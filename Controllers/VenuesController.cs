using Microsoft.AspNetCore.Mvc;
using EventEase.Models;
using System.Collections.Generic;
using System.Linq;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private static List<Venue> venues = new List<Venue>
        {
            new Venue { VenueID = 1, VenueName = "Stadium A", Location = "City X", Capacity = 5000, ImageUrl = "" },
            new Venue { VenueID = 2, VenueName = "Gallery B", Location = "City Y", Capacity = 200, ImageUrl = "" }
        };

        public IActionResult Index()
        {
            return View(venues);
        }

        public IActionResult Details(int id)
        {
            var venue = venues.FirstOrDefault(v => v.VenueID == id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Venue venue)
        {
            venue.VenueID = venues.Count + 1;
            venues.Add(venue);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var venue = venues.FirstOrDefault(v => v.VenueID == id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        [HttpPost]
        public IActionResult Edit(Venue venue)
        {
            var existingVenue = venues.FirstOrDefault(v => v.VenueID == venue.VenueID);

            if (existingVenue != null)
            {
                existingVenue.VenueName = venue.VenueName;
                existingVenue.Location = venue.Location;
                existingVenue.Capacity = venue.Capacity;
                existingVenue.ImageUrl = venue.ImageUrl;
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var venue = venues.FirstOrDefault(v => v.VenueID == id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var venue = venues.FirstOrDefault(v => v.VenueID == id);
            if (venue != null)
            {
                venues.Remove(venue);
            }

            return RedirectToAction("Index");
        }
    }
}