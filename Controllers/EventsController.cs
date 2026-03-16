using Microsoft.AspNetCore.Mvc;
using EventEase.Models;
using System;
using System.Collections.Generic;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        // Dummy data
        private static List<Event> events = new List<Event>
        {
            new Event { EventID = 1, EventName = "Music Concert", EventDate = DateTime.Now.AddDays(5), Description = "Live concert", VenueID = 1 },
            new Event { EventID = 2, EventName = "Art Exhibition", EventDate = DateTime.Now.AddDays(10), Description = "Modern art display", VenueID = 2 }
        };

        public IActionResult Index()
        {
            return View(events);
        }

        public IActionResult Details(int id)
        {
            var ev = events.Find(e => e.EventID == id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Event ev)
        {
            ev.EventID = events.Count + 1;
            events.Add(ev);
            return RedirectToAction("Index");
        }
    }
}