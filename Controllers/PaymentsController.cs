using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.DTOs.Payment;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/payments/initiate - Manual payment
        [HttpPost("initiate")]
        public async Task<ActionResult<ApiResponse<InitiatePaymentResponseDto>>> InitiatePayment([FromBody] InitiatePaymentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<InitiatePaymentResponseDto>.FailureResponse("Invalid input"));

            // Validate booking
            var booking = await _context.Bookings
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId);

            if (booking == null)
                return NotFound(ApiResponse<InitiatePaymentResponseDto>.FailureResponse("Booking not found"));

            if (booking.BookingStatus != BookingStatus.Confirmed)
                return BadRequest(ApiResponse<InitiatePaymentResponseDto>.FailureResponse("Booking is not in confirmed status"));

            // Check if payment already exists
            if (booking.Payment != null && booking.Payment.PaymentStatus == PaymentStatus.Completed)
                return BadRequest(ApiResponse<InitiatePaymentResponseDto>.FailureResponse("Payment already completed for this booking"));

            // Validate amount
            if (request.Amount != booking.TotalFare)
                return BadRequest(ApiResponse<InitiatePaymentResponseDto>.FailureResponse($"Payment amount must match booking total: {booking.TotalFare}"));

            // Create or update payment record
            var payment = booking.Payment ?? new Payment
            {
                PaymentId = Guid.NewGuid(),
                BookingId = booking.BookingId
            };

            payment.Amount = request.Amount;
            payment.PaymentMethod = request.PaymentMethod;
            payment.PaymentStatus = PaymentStatus.Completed; // Manual payment - mark as completed
            payment.TransactionId = GenerateTransactionId();
            payment.PaymentDate = DateTime.UtcNow;

            if (booking.Payment == null)
            {
                _context.Payments.Add(payment);
            }

            await _context.SaveChangesAsync();

            var response = new InitiatePaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod.ToString(),
                PaymentStatus = payment.PaymentStatus.ToString(),
                TransactionId = payment.TransactionId,
                PaymentDate = payment.PaymentDate,
                Message = "Payment processed successfully"
            };

            return Ok(ApiResponse<InitiatePaymentResponseDto>.SuccessResponse(response, "Payment completed"));
        }

        // POST: api/payments/verify
        [HttpPost("verify")]
        public async Task<ActionResult<ApiResponse<VerifyPaymentResponseDto>>> VerifyPayment([FromBody] VerifyPaymentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<VerifyPaymentResponseDto>.FailureResponse("Invalid input"));

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == request.PaymentId);

            if (payment == null)
                return NotFound(ApiResponse<VerifyPaymentResponseDto>.FailureResponse("Payment not found"));

            var isVerified = payment.TransactionId == request.TransactionId
                          && payment.PaymentStatus == PaymentStatus.Completed;

            var response = new VerifyPaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                PaymentStatus = payment.PaymentStatus.ToString(),
                IsVerified = isVerified,
                Message = isVerified ? "Payment verified successfully" : "Payment verification failed"
            };

            return Ok(ApiResponse<VerifyPaymentResponseDto>.SuccessResponse(response));
        }

        // GET: api/payments/{paymentId}
        [HttpGet("{paymentId:guid}")]
        public async Task<ActionResult<ApiResponse<PaymentDetailsDto>>> GetPaymentDetails(Guid paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
                return NotFound(ApiResponse<PaymentDetailsDto>.FailureResponse("Payment not found"));

            var response = new PaymentDetailsDto
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                BookingReference = payment.Booking.BookingReference,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod.ToString(),
                PaymentStatus = payment.PaymentStatus.ToString(),
                TransactionId = payment.TransactionId,
                PaymentDate = payment.PaymentDate
            };

            return Ok(ApiResponse<PaymentDetailsDto>.SuccessResponse(response));
        }

        // POST: api/payments/callback - Manual callback simulation
        [HttpPost("callback")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> PaymentCallback([FromBody] PaymentCallbackRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Invalid input"));

            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.TransactionId == request.TransactionId);

            if (payment == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Transaction not found"));

            payment.PaymentStatus = request.Status;

            // If payment failed, update booking status
            if (request.Status == PaymentStatus.Failed)
            {
                // Optionally, you might want to release the seats
                // payment.Booking.BookingStatus = BookingStatus.Cancelled;
            }

            await _context.SaveChangesAsync();

            var response = new MessageResponse
            {
                Success = true,
                Message = $"Payment status updated to {request.Status}"
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // GET: api/payments/methods
        [HttpGet("methods")]
        public ActionResult<ApiResponse<List<PaymentMethodDto>>> GetPaymentMethods()
        {
            var methods = new List<PaymentMethodDto>
            {
                new() { Method = "Card", DisplayName = "Credit/Debit Card", IsEnabled = true },
                new() { Method = "UPI", DisplayName = "UPI Payment", IsEnabled = true },
                new() { Method = "Wallet", DisplayName = "Wallet", IsEnabled = true },
                new() { Method = "NetBanking", DisplayName = "Net Banking", IsEnabled = true }
            };

            return Ok(ApiResponse<List<PaymentMethodDto>>.SuccessResponse(methods));
        }

        // Helper methods
        private static string GenerateTransactionId()
        {
            return $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }
}
