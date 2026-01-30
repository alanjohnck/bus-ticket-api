using System.ComponentModel.DataAnnotations;
using BusBookingSystem.API.Models;

namespace BusBookingSystem.API.DTOs.Admin
{
    // GET /api/admin/dashboard
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalOperators { get; set; }
        public int PendingOperatorApprovals { get; set; }
        public int TotalBuses { get; set; }
        public int TotalRoutes { get; set; }
        public int TodayTrips { get; set; }
        public int TotalBookingsToday { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int ActiveDisputes { get; set; }
    }

    // User Management
    // GET /api/admin/users
    public class AdminUserListItemDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // GET /api/admin/users/:userId
    public class AdminUserDetailsDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalBookings { get; set; }
        public int TotalReviews { get; set; }
    }

    // PATCH /api/admin/users/:userId/status
    public class UpdateUserStatusRequestDto
    {
        [Required]
        public bool IsActive { get; set; }
    }

    // GET /api/admin/users/statistics
    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int PassengerCount { get; set; }
        public int OperatorCount { get; set; }
        public int AdminCount { get; set; }
        public int NewUsersThisMonth { get; set; }
    }

    // Operator Management
    // GET /api/admin/operators
    public class AdminOperatorListItemDto
    {
        public Guid OperatorId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public bool IsApproved { get; set; }
        public int TotalBuses { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // GET /api/admin/operators/:operatorId
    public class AdminOperatorDetailsDto
    {
        public Guid OperatorId { get; set; }
        public Guid UserId { get; set; }
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
        public int TotalBuses { get; set; }
        public int ActiveBuses { get; set; }
        public int TotalRoutes { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // POST /api/admin/operators/approve
    public class ApproveOperatorRequestDto
    {
        [Required]
        public Guid OperatorId { get; set; }
    }

    // PATCH /api/admin/operators/:operatorId/status
    public class UpdateOperatorStatusRequestDto
    {
        [Required]
        public bool IsApproved { get; set; }
    }

    // GET /api/admin/operators/pending
    public class PendingOperatorDto
    {
        public Guid OperatorId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Analytics
    // GET /api/admin/analytics
    public class AnalyticsQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Metric { get; set; } // bookings, revenue, users, trips
    }

    public class AnalyticsResponseDto
    {
        public string Metric { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalValue { get; set; }
        public decimal PreviousPeriodValue { get; set; }
        public decimal ChangePercentage { get; set; }
        public List<AnalyticsDataPointDto> DataPoints { get; set; } = new();
    }

    public class AnalyticsDataPointDto
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }

    // Reports
    // GET /api/admin/reports
    public class ReportQueryDto
    {
        public string ReportType { get; set; } = string.Empty; // bookings, revenue, operators, users
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ReportResponseDto
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public object Data { get; set; } = new();
    }

    // Booking Management
    // GET /api/admin/bookings
    public class AdminBookingListItemDto
    {
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public DateTime TravelDate { get; set; }
        public int TotalSeats { get; set; }
        public decimal TotalFare { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }

    // GET /api/admin/bookings/statistics
    public class BookingStatisticsDto
    {
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageBookingValue { get; set; }
        public int BookingsToday { get; set; }
        public int BookingsThisWeek { get; set; }
        public int BookingsThisMonth { get; set; }
    }

    // PATCH /api/admin/bookings/:bookingId
    public class AdminModifyBookingRequestDto
    {
        public BookingStatus? BookingStatus { get; set; }
        public string? Notes { get; set; }
    }

    // Payment Management
    // GET /api/admin/payments
    public class AdminPaymentListItemDto
    {
        public Guid PaymentId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }

    // GET /api/admin/payments/failed
    public class FailedPaymentDto
    {
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }

    // GET /api/admin/payments/reconcile
    public class PaymentReconciliationDto
    {
        public DateTime Date { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public int CompletedCount { get; set; }
        public decimal CompletedAmount { get; set; }
        public int PendingCount { get; set; }
        public decimal PendingAmount { get; set; }
        public int FailedCount { get; set; }
        public decimal FailedAmount { get; set; }
    }

    // Refund Management
    // POST /api/admin/refunds/:refundId/approve
    public class ApproveRefundRequestDto
    {
        public string? Notes { get; set; }
    }

    // POST /api/admin/refunds/:refundId/reject
    public class RejectRefundRequestDto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    // Content Management - Offers
    // POST /api/admin/offers
    public class CreateOfferRequestDto
    {
        [Required]
        public string OfferCode { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DiscountType DiscountType { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal DiscountValue { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal MinBookingAmount { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal MaxDiscount { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int UsageLimit { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // PUT /api/admin/offers/:offerId
    public class UpdateOfferRequestDto
    {
        public string? Description { get; set; }
        public DiscountType? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? MinBookingAmount { get; set; }
        public decimal? MaxDiscount { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public int? UsageLimit { get; set; }
        public bool? IsActive { get; set; }
    }

    // GET /api/admin/offers
    public class AdminOfferListItemDto
    {
        public Guid OfferId { get; set; }
        public string OfferCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal MinBookingAmount { get; set; }
        public decimal MaxDiscount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int UsageLimit { get; set; }
        public int TimesUsed { get; set; }
        public bool IsActive { get; set; }
    }

    // Dispute Management
    // GET /api/admin/disputes
    public class DisputeListItemDto
    {
        public Guid DisputeId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // GET /api/admin/disputes/:disputeId
    public class DisputeDetailsDto
    {
        public Guid DisputeId { get; set; }
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Resolution { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    // PATCH /api/admin/disputes/:disputeId
    public class UpdateDisputeStatusRequestDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }

    // POST /api/admin/disputes/:disputeId/resolve
    public class ResolveDisputeRequestDto
    {
        [Required]
        public string Resolution { get; set; } = string.Empty;

        public decimal? RefundAmount { get; set; }
    }
}
