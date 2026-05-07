using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        public int VenueID { get; set; }

        [Required(ErrorMessage = "Venue name is required")]
        [StringLength(100)]
        public string? VenueName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        [StringLength(100)]
        public string? Location { get; set; } = string.Empty;

        [Required]
        [Range(1, 100000, ErrorMessage = "Capacity must be greater than 0")]
        public int Capacity { get; set; }

        [Display(Name = "Image")]
        public string? ImageUrl { get; set; }
    }
}
