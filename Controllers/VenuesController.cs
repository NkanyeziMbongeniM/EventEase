using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly EventEaseContext _context;
        private readonly BlobStorageService _blobStorageService;

        public VenuesController(EventEaseContext context, BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var venues = from v in _context.Venues select v;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                venues = venues.Where(v => v.VenueName.Contains(searchString) || v.Location.Contains(searchString));
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
        public async Task<IActionResult> Create(Venue venue, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    venue.ImageUrl = await _blobStorageService.UploadFileAsync(imageFile);
                }

                if (string.IsNullOrEmpty(venue.ImageUrl))
                {
                    venue.ImageUrl = "/images/defaultvenue.jpg";
                }

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
        public async Task<IActionResult> Edit(int id, Venue venue, IFormFile? imageFile)
        {
            if (id != venue.VenueID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVenue = await _context.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.VenueID == id);
                    if (existingVenue == null) return NotFound();

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        venue.ImageUrl = await _blobStorageService.UploadFileAsync(imageFile);
                    }
                    else
                    {
                        venue.ImageUrl = existingVenue.ImageUrl;
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Venue updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Venues.AnyAsync(e => e.VenueID == venue.VenueID)) return NotFound();
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

            bool hasBookings = await _context.Bookings.AnyAsync(b => b.VenueID == id);
            bool hasEvents = await _context.Events.AnyAsync(e => e.VenueID == id);

            if (hasBookings || hasEvents)
            {
                TempData["Error"] = "This venue cannot be deleted because it is linked to existing events or bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Venue deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
