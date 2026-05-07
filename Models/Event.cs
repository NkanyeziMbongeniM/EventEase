using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Event
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required")]
        public DateTime EventDate { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a venue")]
        public int VenueID { get; set; }

        [ForeignKey("VenueID")]
        public Venue? Venue { get; set; }
    }
}
