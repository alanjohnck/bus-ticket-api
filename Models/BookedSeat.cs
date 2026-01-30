using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.Models
{
    public class BookedSeat
    {
        [Key]
        public Guid BookedSeatId { get; set; }
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }

        public string SeatNumber { get; set; }
        public string PassengerName { get; set; }
        public int PassengerAge { get; set; }
        public string PassengerGender { get; set; }

        public Guid BoardingPointId { get; set; } // FK to Stop
        public Guid DroppingPointId { get; set; } // FK to Stop
    }
}