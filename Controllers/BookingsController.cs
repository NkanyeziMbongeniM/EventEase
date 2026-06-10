using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly EventEaseContext _context;

        public BookingsController(EventEaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString, int? eventTypeId, DateTime? startDate, DateTime? endDate, bool? isAvailable)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentEventTypeId"] = eventTypeId;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentAvailability"] = isAvailable;
            ViewBag.EventTypes = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeID", "EventTypeName", eventTypeId);

            var bookings = _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e!.EventType)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                bookings = bookings.Where(b =>
                    b.BookingID.ToString().Contains(searchString) ||
                    (b.Event != null && b.Event.EventName.Contains(searchString)) ||
                    (b.Venue != null && b.Venue.VenueName.Contains(searchString)));
            }

            if (eventTypeId.HasValue)
            {
                bookings = bookings.Where(b => b.Event != null && b.Event.EventTypeID == eventTypeId.Value);
            }

            if (startDate.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate.Date <= endDate.Value.Date);
            }

            if (isAvailable.HasValue)
            {
                bookings = bookings.Where(b => b.Venue != null && b.Venue.IsAvailable == isAvailable.Value);
            }

            return View(await bookings.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e!.EventType)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound();
            return View(booking);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var doubleBooking = await _context.Bookings.AnyAsync(b =>
                b.BookingID != booking.BookingID &&
                b.VenueID == booking.VenueID &&
                b.BookingDate == booking.BookingDate);

            if (doubleBooking)
            {
                ModelState.AddModelError(string.Empty, "This venue is already booked for the selected date and time.");
            }

            var venue = await _context.Venues.FindAsync(booking.VenueID);
            if (venue != null && !venue.IsAvailable)
            {
                ModelState.AddModelError(string.Empty, "This venue is currently marked as unavailable.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking created successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropDowns(booking.EventID, booking.VenueID);
            return View(booking);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            await PopulateDropDowns(booking.EventID, booking.VenueID);
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingID) return NotFound();

            var doubleBooking = await _context.Bookings.AnyAsync(b =>
                b.BookingID != booking.BookingID &&
                b.VenueID == booking.VenueID &&
                b.BookingDate == booking.BookingDate);

            if (doubleBooking)
            {
                ModelState.AddModelError(string.Empty, "This venue is already booked for the selected date and time.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Booking updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await BookingExists(booking.BookingID)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropDowns(booking.EventID, booking.VenueID);
            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropDowns(int? selectedEvent = null, int? selectedVenue = null)
        {
            ViewBag.EventsList = new SelectList(await _context.Events.Include(e => e.EventType).ToListAsync(), "EventID", "EventName", selectedEvent);
            ViewBag.VenuesList = new SelectList(await _context.Venues.Where(v => v.IsAvailable).ToListAsync(), "VenueID", "VenueName", selectedVenue);
        }

        private async Task<bool> BookingExists(int id)
        {
            return await _context.Bookings.AnyAsync(e => e.BookingID == id);
        }
    }
}
