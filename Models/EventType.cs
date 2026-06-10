using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class EventType
    {
        public int EventTypeID { get; set; }

        [Required]
        [StringLength(100)]
        public string EventTypeName { get; set; } = string.Empty;

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
