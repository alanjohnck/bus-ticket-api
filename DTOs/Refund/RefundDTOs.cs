using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.DTOs.Refund
{
    // POST /api/refunds
    public class CreateRefundRequestDto
    {
        [Required]
        public Guid BookingId { get; set; }

        public string? Reason { get; set; }
    }

    public class CreateRefundResponseDto
    {
        public Guid RefundId { get; set; }
        public Guid BookingId { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // GET /api/refunds/:refundId
    public class RefundDetailsDto
    {
        public Guid CancellationId { get; set; }
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public decimal CancellationCharges { get; set; }
        public string RefundStatus { get; set; } = string.Empty;
        public DateTime CancellationDate { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
