using Microsoft.EntityFrameworkCore;

namespace EventEase.Models
{
    public class EventEaseContext : DbContext
    {
        public EventEaseContext(DbContextOptions<EventEaseContext> options)
            : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EventType> EventTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeID = 1, EventTypeName = "Conference" },
                new EventType { EventTypeID = 2, EventTypeName = "Wedding" },
                new EventType { EventTypeID = 3, EventTypeName = "Concert" },
                new EventType { EventTypeID = 4, EventTypeName = "Exhibition" },
                new EventType { EventTypeID = 5, EventTypeName = "Corporate Event" }
            );

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
