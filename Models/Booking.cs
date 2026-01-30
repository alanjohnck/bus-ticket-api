using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.Models
{
    public class Booking
    {
        [Key]
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid TripId { get; set; }
        public Trip Trip { get; set; }

        public string BookingReference { get; set; }
        public int TotalSeats { get; set; }
        public decimal TotalFare { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public string PassengerDetailsJson { get; set; } // [cite: 317]

        // Navigation
        public ICollection<BookedSeat> BookedSeats { get; set; }
        public Payment Payment { get; set; }
        public Cancellation Cancellation { get; set; }
    }
}