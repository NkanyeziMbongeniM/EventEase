using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Booking
    {
        public int BookingID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        public int VenueID { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [ForeignKey("EventID")]
        public Event? Event { get; set; }

        [ForeignKey("VenueID")]
        public Venue? Venue { get; set; }
    }
}
