using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventEaseContext _context;

        public EventsController(EventEaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString, int? eventTypeId, DateTime? startDate, DateTime? endDate)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentEventTypeId"] = eventTypeId;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");

            ViewBag.EventTypes = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeID", "EventTypeName", eventTypeId);

            var events = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                events = events.Where(e => e.EventName.Contains(searchString) ||
                    (e.Description != null && e.Description.Contains(searchString)) ||
                    (e.Venue != null && e.Venue.VenueName.Contains(searchString)));
            }

            if (eventTypeId.HasValue)
            {
                events = events.Where(e => e.EventTypeID == eventTypeId.Value);
            }

            if (startDate.HasValue)
            {
                events = events.Where(e => e.EventDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                events = events.Where(e => e.EventDate.Date <= endDate.Value.Date);
            }

            return View(await events.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null) return NotFound();
            return View(ev);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event ev)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ev);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event created successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropDowns(ev.VenueID, ev.EventTypeID);
            return View(ev);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            await PopulateDropDowns(ev.VenueID, ev.EventTypeID);
            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event ev)
        {
            if (id != ev.EventID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ev);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Event updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await EventExists(ev.EventID)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropDowns(ev.VenueID, ev.EventTypeID);
            return View(ev);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null) return NotFound();
            return View(ev);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return RedirectToAction(nameof(Index));

            var hasBookings = await _context.Bookings.AnyAsync(b => b.EventID == id);
            if (hasBookings)
            {
                TempData["Error"] = "This event cannot be deleted because it is linked to an existing booking.";
                return RedirectToAction(nameof(Index));
            }

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Event deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropDowns(int? selectedVenue = null, int? selectedEventType = null)
        {
            ViewBag.Venues = new SelectList(await _context.Venues.ToListAsync(), "VenueID", "VenueName", selectedVenue);
            ViewBag.EventTypes = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeID", "EventTypeName", selectedEventType);
        }

        private async Task<bool> EventExists(int id)
        {
            return await _context.Events.AnyAsync(e => e.EventID == id);
        }
    }
}
