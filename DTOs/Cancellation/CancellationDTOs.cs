using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.DTOs.Cancellation
{
    // POST /api/cancellations
    public class CreateCancellationRequestDto
    {
        [Required]
        public Guid BookingId { get; set; }

        public string? Reason { get; set; }
    }

    public class CreateCancellationResponseDto
    {
        public Guid CancellationId { get; set; }
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public decimal CancellationCharges { get; set; }
        public string RefundStatus { get; set; } = string.Empty;
        public DateTime CancellationDate { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // GET /api/cancellations/:cancellationId
    public class CancellationDetailsDto
    {
        public Guid CancellationId { get; set; }
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string CancellationReason { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public decimal CancellationCharges { get; set; }
        public DateTime CancellationDate { get; set; }
        public string RefundStatus { get; set; } = string.Empty;
        public CancelledByDto CancelledBy { get; set; } = new();
    }

    public class CancelledByDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    // GET /api/cancellations/policy
    public class CancellationPolicyDto
    {
        public List<CancellationSlabDto> Slabs { get; set; } = new();
        public string PolicyDescription { get; set; } = string.Empty;
    }

    public class CancellationSlabDto
    {
        public int HoursBeforeDeparture { get; set; }
        public decimal CancellationChargePercentage { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    // POST /api/cancellations/calculate
    public class CalculateRefundRequestDto
    {
        [Required]
        public Guid BookingId { get; set; }
    }

    public class CalculateRefundResponseDto
    {
        public Guid BookingId { get; set; }
        public decimal TotalFare { get; set; }
        public decimal CancellationCharges { get; set; }
        public decimal RefundAmount { get; set; }
        public string AppliedSlab { get; set; } = string.Empty;
    }
}
