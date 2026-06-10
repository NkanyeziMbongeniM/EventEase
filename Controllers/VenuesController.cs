using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly EventEaseContext _context;

        public VenuesController(EventEaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString, bool? isAvailable)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentAvailability"] = isAvailable;

            var venues = _context.Venues.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                venues = venues.Where(v => v.VenueName.Contains(searchString) || v.Location.Contains(searchString));
            }

            if (isAvailable.HasValue)
            {
                venues = venues.Where(v => v.IsAvailable == isAvailable.Value);
            }

            return View(await venues.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueID == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue)
        {
            if (ModelState.IsValid)
            {
                _context.Add(venue);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Venue created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue)
        {
            if (id != venue.VenueID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Venue updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await VenueExists(venue.VenueID)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueID == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return RedirectToAction(nameof(Index));

            var hasBookings = await _context.Bookings.AnyAsync(b => b.VenueID == id);
            var hasEvents = await _context.Events.AnyAsync(e => e.VenueID == id);

            if (hasBookings || hasEvents)
            {
                TempData["Error"] = "This venue cannot be deleted because it is linked to an existing event or booking.";
                return RedirectToAction(nameof(Index));
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Venue deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> VenueExists(int id)
        {
            return await _context.Venues.AnyAsync(e => e.VenueID == id);
        }
    }
}
