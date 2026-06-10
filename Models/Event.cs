using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Event
    {
        public int EventID { get; set; }

        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        public int VenueID { get; set; }

        [ForeignKey("VenueID")]
        public Venue? Venue { get; set; }

        public int? EventTypeID { get; set; }

        [ForeignKey("EventTypeID")]
        public EventType? EventType { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
