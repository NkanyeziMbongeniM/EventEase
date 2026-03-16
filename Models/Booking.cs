using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Booking
    {
        public int BookingID { get; set; }

        public int EventID { get; set; }

        public int VenueID { get; set; }

        public DateTime BookingDate { get; set; }

        [ForeignKey("EventID")]
        public Event Event { get; set; }

        [ForeignKey("VenueID")]
        public Venue Venue { get; set; }
    }
}