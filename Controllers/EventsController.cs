using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventEaseContext _context;

        public EventsController(EventEaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var events = _context.Events.Include(e => e.Venue).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                events = events.Where(e => e.EventName.Contains(searchString) || (e.Description != null && e.Description.Contains(searchString)) || e.Venue!.VenueName.Contains(searchString));
            }

            return View(await events.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Events.Include(e => e.Venue).FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateVenuesDropDownList();
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
            await PopulateVenuesDropDownList(ev.VenueID);
            return View(ev);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            await PopulateVenuesDropDownList(ev.VenueID);
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
                    if (!await _context.Events.AnyAsync(e => e.EventID == ev.EventID)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateVenuesDropDownList(ev.VenueID);
            return View(ev);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Events.Include(e => e.Venue).FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return RedirectToAction(nameof(Index));

            bool hasBookings = await _context.Bookings.AnyAsync(b => b.EventID == id);
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

        private async Task PopulateVenuesDropDownList(object? selectedVenue = null)
        {
            var venues = await _context.Venues.OrderBy(v => v.VenueName).ToListAsync();
            ViewBag.VenueID = new SelectList(venues, "VenueID", "VenueName", selectedVenue);
        }
    }
}
