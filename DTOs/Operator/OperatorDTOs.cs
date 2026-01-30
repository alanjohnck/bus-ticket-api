using System.ComponentModel.DataAnnotations;
using BusBookingSystem.API.Models;

namespace BusBookingSystem.API.DTOs.Operator
{
    // GET /api/operator/dashboard
    public class OperatorDashboardDto
    {
        public Guid OperatorId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int TotalBuses { get; set; }
        public int ActiveBuses { get; set; }
        public int TotalRoutes { get; set; }
        public int TotalSchedules { get; set; }
        public int TodayTrips { get; set; }
        public int TotalBookingsToday { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    // GET/PUT /api/operator/profile
    public class OperatorProfileDto
    {
        public Guid OperatorId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateOperatorProfileRequestDto
    {
        [Required]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        public string ContactPhone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }

    // Fleet Management - Buses
    // POST /api/operator/buses
    public class CreateBusRequestDto
    {
        [Required]
        public string BusNumber { get; set; } = string.Empty;

        [Required]
        public BusType BusType { get; set; }

        [Required]
        public BusCategory BusCategory { get; set; }

        [Required, Range(1, 100)]
        public int TotalSeats { get; set; }

        [Required]
        public string RegistrationNumber { get; set; } = string.Empty;

        public List<string> Amenities { get; set; } = new();
    }

    public class CreateBusResponseDto
    {
        public Guid BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // GET /api/operator/buses
    public class OperatorBusListItemDto
    {
        public Guid BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string BusType { get; set; } = string.Empty;
        public string BusCategory { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> Amenities { get; set; } = new();
    }

    // PUT /api/operator/buses/:busId
    public class UpdateBusRequestDto
    {
        public BusType? BusType { get; set; }
        public BusCategory? BusCategory { get; set; }
        public int? TotalSeats { get; set; }
        public string? RegistrationNumber { get; set; }
        public List<string>? Amenities { get; set; }
    }

    // PATCH /api/operator/buses/:busId/status
    public class UpdateBusStatusRequestDto
    {
        [Required]
        public bool IsActive { get; set; }
    }

    // Seat Layout Management
    // POST /api/operator/buses/:busId/seats
    public class CreateSeatLayoutRequestDto
    {
        [Required]
        public List<SeatLayoutItemDto> Seats { get; set; } = new();
    }

    public class SeatLayoutItemDto
    {
        [Required]
        public string SeatNumber { get; set; } = string.Empty;

        [Required]
        public SeatType SeatType { get; set; }

        [Required]
        public DeckType Deck { get; set; }

        [Required]
        public int PositionX { get; set; }

        [Required]
        public int PositionY { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    // PUT /api/operator/buses/:busId/seats/:seatId
    public class UpdateSeatRequestDto
    {
        public string? SeatNumber { get; set; }
        public SeatType? SeatType { get; set; }
        public DeckType? Deck { get; set; }
        public int? PositionX { get; set; }
        public int? PositionY { get; set; }
        public bool? IsAvailable { get; set; }
    }

    // Route Management
    // POST /api/operator/routes
    public class CreateRouteRequestDto
    {
        [Required]
        public string SourceCity { get; set; } = string.Empty;

        [Required]
        public string DestinationCity { get; set; } = string.Empty;

        [Required, Range(1, 10000)]
        public decimal DistanceKm { get; set; }

        [Required, Range(0.1, 100)]
        public decimal EstimatedDurationHours { get; set; }

        public List<CreateStopDto>? Stops { get; set; }
    }

    public class CreateStopDto
    {
        [Required]
        public string StopName { get; set; } = string.Empty;

        [Required]
        public int StopOrder { get; set; }

        [Required]
        public TimeSpan ArrivalTimeOffset { get; set; }

        [Required]
        public TimeSpan DepartureTimeOffset { get; set; }
    }

    public class CreateRouteResponseDto
    {
        public Guid RouteId { get; set; }
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // GET /api/operator/routes
    public class OperatorRouteListItemDto
    {
        public Guid RouteId { get; set; }
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal EstimatedDurationHours { get; set; }
        public int StopsCount { get; set; }
    }

    // PUT /api/operator/routes/:routeId
    public class UpdateRouteRequestDto
    {
        public string? SourceCity { get; set; }
        public string? DestinationCity { get; set; }
        public decimal? DistanceKm { get; set; }
        public decimal? EstimatedDurationHours { get; set; }
    }

    // Schedule Management
    // POST /api/operator/schedules
    public class CreateScheduleRequestDto
    {
        [Required]
        public Guid BusId { get; set; }

        [Required]
        public Guid RouteId { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal BaseFare { get; set; }

        public List<DateTime>? AvailableDates { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CreateScheduleResponseDto
    {
        public Guid ScheduleId { get; set; }
        public Guid BusId { get; set; }
        public Guid RouteId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // GET /api/operator/schedules
    public class OperatorScheduleListItemDto
    {
        public Guid ScheduleId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal BaseFare { get; set; }
        public bool IsActive { get; set; }
    }

    // GET /api/operator/schedules/:scheduleId
    public class OperatorScheduleDetailsDto
    {
        public Guid ScheduleId { get; set; }
        public Guid BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public Guid RouteId { get; set; }
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal BaseFare { get; set; }
        public List<DateTime> AvailableDates { get; set; } = new();
        public bool IsActive { get; set; }
    }

    // PUT /api/operator/schedules/:scheduleId
    public class UpdateScheduleRequestDto
    {
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public decimal? BaseFare { get; set; }
        public List<DateTime>? AvailableDates { get; set; }
    }

    // PATCH /api/operator/schedules/:scheduleId/status
    public class UpdateScheduleStatusRequestDto
    {
        [Required]
        public bool IsActive { get; set; }
    }

    // Trip Management
    // GET /api/operator/trips
    public class OperatorTripListItemDto
    {
        public Guid TripId { get; set; }
        public Guid ScheduleId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public DateTime TripDate { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public int AvailableSeats { get; set; }
        public int BookedSeats { get; set; }
    }

    // GET /api/operator/trips/:tripId
    public class OperatorTripDetailsDto
    {
        public Guid TripId { get; set; }
        public Guid ScheduleId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public DateTime TripDate { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public int BookedSeats { get; set; }
        public decimal Revenue { get; set; }
    }

    // PATCH /api/operator/trips/:tripId/status
    public class UpdateTripStatusRequestDto
    {
        [Required]
        public TripStatus CurrentStatus { get; set; }
    }

    // POST /api/operator/trips/:tripId/cancel
    public class CancelTripRequestDto
    {
        public string? Reason { get; set; }
    }

    // GET /api/operator/trips/:tripId/bookings
    public class TripBookingListItemDto
    {
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public decimal TotalFare { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }

    // GET /api/operator/trips/:tripId/passengers
    public class TripPassengerDto
    {
        public string SeatNumber { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public int PassengerAge { get; set; }
        public string PassengerGender { get; set; } = string.Empty;
        public string BoardingPoint { get; set; } = string.Empty;
        public string DroppingPoint { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
    }

    // Revenue
    // GET /api/operator/revenue
    public class RevenueQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? GroupBy { get; set; } // day, week, month
    }

    public class RevenueStatisticsDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int TotalPassengers { get; set; }
        public int CancelledBookings { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal NetRevenue { get; set; }
        public List<RevenueBreakdownDto> Breakdown { get; set; } = new();
    }

    public class RevenueBreakdownDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Bookings { get; set; }
    }

    // Operator Bookings
    // GET /api/operator/bookings
    public class OperatorBookingListItemDto
    {
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public DateTime TravelDate { get; set; }
        public int TotalSeats { get; set; }
        public decimal TotalFare { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }

    // Reviews Management
    // GET /api/operator/reviews
    public class OperatorReviewListItemDto
    {
        public Guid ReviewId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public DateTime TravelDate { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string? Response { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // GET /api/operator/reviews/stats
    public class ReviewStatisticsDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
    }

    // POST /api/operator/reviews/:reviewId/respond
    public class RespondToReviewRequestDto
    {
        [Required]
        public string Response { get; set; } = string.Empty;
    }
}
