using Microsoft.AspNetCore.Mvc;
using EventEase.Models;
using System.Collections.Generic;
using System.Linq;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        // Temporary in-memory lists
        private static List<Booking> bookings = new List<Booking>();
        private static List<Event> events = new List<Event>
        {
            new Event { EventID = 1, EventName = "Concert X", EventDate = System.DateTime.Now.AddDays(10), Description = "Music Concert" },
            new Event { EventID = 2, EventName = "Exhibition Y", EventDate = System.DateTime.Now.AddDays(20), Description = "Art Exhibition" }
        };

        private static List<Venue> venues = new List<Venue>
        {
            new Venue { VenueID = 1, VenueName = "Stadium A", Location = "City X", Capacity = 5000, ImageUrl = "" },
            new Venue { VenueID = 2, VenueName = "Gallery B", Location = "City Y", Capacity = 200, ImageUrl = "" }
        };

        // GET: Bookings
        public IActionResult Index()
        {
            var model = bookings;
            return View(model);
        }

        // GET: Bookings/Details/5
        public IActionResult Details(int id)
        {
            var booking = bookings.FirstOrDefault(b => b.BookingID == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewBag.EventsList = events;
            ViewBag.VenuesList = venues;
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        public IActionResult Create(Booking booking)
        {
            booking.BookingID = bookings.Count + 1;
            bookings.Add(booking);
            return RedirectToAction("Index");
        }

        // GET: Bookings/Edit/5
        public IActionResult Edit(int id)
        {
            var booking = bookings.FirstOrDefault(b => b.BookingID == id);
            if (booking == null) return NotFound();

            ViewBag.EventsList = events;
            ViewBag.VenuesList = venues;

            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        public IActionResult Edit(Booking booking)
        {
            var existingBooking = bookings.FirstOrDefault(b => b.BookingID == booking.BookingID);
            if (existingBooking != null)
            {
                existingBooking.EventID = booking.EventID;
                existingBooking.VenueID = booking.VenueID;
                existingBooking.BookingDate = booking.BookingDate;
            }

            return RedirectToAction("Index");
        }

        // GET: Bookings/Delete/5
        public IActionResult Delete(int id)
        {
            var booking = bookings.FirstOrDefault(b => b.BookingID == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var booking = bookings.FirstOrDefault(b => b.BookingID == id);
            if (booking != null)
            {
                bookings.Remove(booking);
            }

            return RedirectToAction("Index");
        }
    }
}