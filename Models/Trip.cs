using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.Models
{
    public class Trip
    {
        [Key]
        public Guid TripId { get; set; }
        public Guid ScheduleId { get; set; }
        public Schedule Schedule { get; set; }

        public DateTime TripDate { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public TripStatus CurrentStatus { get; set; }
        public int AvailableSeats { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Booking> Bookings { get; set; }
    }
}