using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly EventEaseContext _context;

        public BookingsController(EventEaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var bookings = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                bookings = bookings.Where(b =>
                    b.BookingID.ToString().Contains(searchString) ||
                    b.Event!.EventName.Contains(searchString) ||
                    b.Venue!.VenueName.Contains(searchString));
            }

            return View(await bookings.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropDownLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            bool doubleBooking = await _context.Bookings.AnyAsync(b =>
                b.VenueID == booking.VenueID &&
                b.BookingDate.Date == booking.BookingDate.Date);

            if (doubleBooking)
            {
                ModelState.AddModelError(string.Empty,
                    "This venue is already booked for the selected date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Booking created successfully.";

                return RedirectToAction(nameof(Index));
            }

            await PopulateDropDownLists(booking.EventID, booking.VenueID);

            return View(booking);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null) return NotFound();

            await PopulateDropDownLists(booking.EventID, booking.VenueID);

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingID)
                return NotFound();

            bool doubleBooking = await _context.Bookings.AnyAsync(b =>
                b.BookingID != booking.BookingID &&
                b.VenueID == booking.VenueID &&
                b.BookingDate.Date == booking.BookingDate.Date);

            if (doubleBooking)
            {
                ModelState.AddModelError(string.Empty,
                    "This venue is already booked for the selected date.");
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
                    if (!await _context.Bookings.AnyAsync(e =>
                        e.BookingID == booking.BookingID))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await PopulateDropDownLists(booking.EventID, booking.VenueID);

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

        private async Task PopulateDropDownLists(
            object? selectedEvent = null,
            object? selectedVenue = null)
        {
            ViewBag.EventID = new SelectList(
                await _context.Events
                    .OrderBy(e => e.EventName)
                    .ToListAsync(),
                "EventID",
                "EventName",
                selectedEvent);

            ViewBag.VenueID = new SelectList(
                await _context.Venues
                    .OrderBy(v => v.VenueName)
                    .ToListAsync(),
                "VenueID",
                "VenueName",
                selectedVenue);
        }
    }
}