using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.DTOs.Booking
{
    // POST /api/bookings
    public class CreateBookingRequestDto
    {
        [Required]
        public Guid TripId { get; set; }

        [Required, MinLength(1)]
        public List<string> Seats { get; set; } = new();

        [Required, MinLength(1)]
        public List<PassengerDto> Passengers { get; set; } = new();

        [Required]
        public Guid BoardingPoint { get; set; }

        [Required]
        public Guid DroppingPoint { get; set; }

        public string? OfferCode { get; set; }
    }

    public class PassengerDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, Range(1, 120)]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public string SeatNumber { get; set; } = string.Empty;
    }

    public class CreateBookingResponseDto
    {
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public decimal TotalFare { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // POST /api/bookings/hold-seats
    public class HoldSeatsRequestDto
    {
        [Required]
        public Guid TripId { get; set; }

        [Required, MinLength(1)]
        public List<string> Seats { get; set; } = new();
    }

    public class HoldSeatsResponseDto
    {
        public string HoldId { get; set; } = string.Empty;
        public Guid TripId { get; set; }
        public List<string> HeldSeats { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }

    // DELETE /api/bookings/release-seats
    public class ReleaseSeatsRequestDto
    {
        [Required]
        public string HoldId { get; set; } = string.Empty;
    }

    // GET /api/bookings/:bookingId
    public class BookingDetailsDto
    {
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public decimal TotalFare { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public TripSummaryDto Trip { get; set; } = new();
        public List<BookedSeatDto> BookedSeats { get; set; } = new();
        public PaymentSummaryDto? Payment { get; set; }
    }

    public class TripSummaryDto
    {
        public Guid TripId { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
    }

    public class BookedSeatDto
    {
        public string SeatNumber { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public int PassengerAge { get; set; }
        public string PassengerGender { get; set; } = string.Empty;
        public string BoardingPoint { get; set; } = string.Empty;
        public string DroppingPoint { get; set; } = string.Empty;
    }

    public class PaymentSummaryDto
    {
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }

    // PATCH /api/bookings/:bookingId
    public class ModifyBookingRequestDto
    {
        public Guid? BoardingPoint { get; set; }
        public Guid? DroppingPoint { get; set; }
    }

    // GET /api/bookings - List
    public class BookingListItemDto
    {
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime TravelDate { get; set; }
        public int TotalSeats { get; set; }
        public decimal TotalFare { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }
}
