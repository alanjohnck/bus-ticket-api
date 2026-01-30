using System.ComponentModel.DataAnnotations;
using BusBookingSystem.API.Models;

namespace BusBookingSystem.API.DTOs.Payment
{
    // POST /api/payments/initiate - Manual payment
    public class InitiatePaymentRequestDto
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public string? TransactionReference { get; set; }
    }

    public class InitiatePaymentResponseDto
    {
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // POST /api/payments/verify
    public class VerifyPaymentRequestDto
    {
        [Required]
        public Guid PaymentId { get; set; }

        [Required]
        public string TransactionId { get; set; } = string.Empty;
    }

    public class VerifyPaymentResponseDto
    {
        public Guid PaymentId { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // GET /api/payments/:paymentId
    public class PaymentDetailsDto
    {
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }

    // GET /api/payments/methods
    public class PaymentMethodDto
    {
        public string Method { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }

    // POST /api/payments/callback - Manual callback simulation
    public class PaymentCallbackRequestDto
    {
        [Required]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        public PaymentStatus Status { get; set; }

        public string? Remarks { get; set; }
    }
}
