using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.DTOs.Refund;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefundsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RefundsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/refunds
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CreateRefundResponseDto>>> RequestRefund([FromBody] CreateRefundRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CreateRefundResponseDto>.FailureResponse("Invalid input"));

            var booking = await _context.Bookings
                .Include(b => b.Cancellation)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId);

            if (booking == null)
                return NotFound(ApiResponse<CreateRefundResponseDto>.FailureResponse("Booking not found"));

            if (booking.Cancellation == null)
                return BadRequest(ApiResponse<CreateRefundResponseDto>.FailureResponse("Booking must be cancelled before requesting refund"));

            if (booking.Cancellation.RefundStatus == RefundStatus.Processed)
                return BadRequest(ApiResponse<CreateRefundResponseDto>.FailureResponse("Refund already processed"));

            if (booking.Payment == null || booking.Payment.PaymentStatus != PaymentStatus.Refunded)
                return BadRequest(ApiResponse<CreateRefundResponseDto>.FailureResponse("No completed payment found for this booking"));

            // Update refund status to pending (for admin approval)
            booking.Cancellation.RefundStatus = RefundStatus.Pending;
            await _context.SaveChangesAsync();

            var response = new CreateRefundResponseDto
            {
                RefundId = booking.Cancellation.CancellationId,
                BookingId = booking.BookingId,
                RefundAmount = booking.Cancellation.RefundAmount,
                RefundStatus = booking.Cancellation.RefundStatus.ToString(),
                Message = "Refund request submitted. It will be processed within 5-7 business days."
            };

            return Ok(ApiResponse<CreateRefundResponseDto>.SuccessResponse(response, "Refund requested"));
        }

        // GET: api/refunds/{refundId}
        [HttpGet("{refundId:guid}")]
        public async Task<ActionResult<ApiResponse<RefundDetailsDto>>> GetRefundDetails(Guid refundId)
        {
            var cancellation = await _context.Cancellations
                .Include(c => c.Booking)
                .FirstOrDefaultAsync(c => c.CancellationId == refundId);

            if (cancellation == null)
                return NotFound(ApiResponse<RefundDetailsDto>.FailureResponse("Refund not found"));

            var response = new RefundDetailsDto
            {
                CancellationId = cancellation.CancellationId,
                BookingId = cancellation.BookingId,
                BookingReference = cancellation.Booking.BookingReference,
                RefundAmount = cancellation.RefundAmount,
                CancellationCharges = cancellation.CancellationCharges,
                RefundStatus = cancellation.RefundStatus.ToString(),
                CancellationDate = cancellation.CancellationDate,
                ProcessedAt = cancellation.RefundStatus == RefundStatus.Processed ? cancellation.CreatedAt : null
            };

            return Ok(ApiResponse<RefundDetailsDto>.SuccessResponse(response));
        }

        // GET: api/refunds/booking/{bookingId}
        [HttpGet("booking/{bookingId:guid}")]
        public async Task<ActionResult<ApiResponse<RefundDetailsDto>>> GetRefundByBooking(Guid bookingId)
        {
            var cancellation = await _context.Cancellations
                .Include(c => c.Booking)
                .FirstOrDefaultAsync(c => c.BookingId == bookingId);

            if (cancellation == null)
                return NotFound(ApiResponse<RefundDetailsDto>.FailureResponse("No refund found for this booking"));

            var response = new RefundDetailsDto
            {
                CancellationId = cancellation.CancellationId,
                BookingId = cancellation.BookingId,
                BookingReference = cancellation.Booking.BookingReference,
                RefundAmount = cancellation.RefundAmount,
                CancellationCharges = cancellation.CancellationCharges,
                RefundStatus = cancellation.RefundStatus.ToString(),
                CancellationDate = cancellation.CancellationDate,
                ProcessedAt = cancellation.RefundStatus == RefundStatus.Processed ? cancellation.CreatedAt : null
            };

            return Ok(ApiResponse<RefundDetailsDto>.SuccessResponse(response));
        }
    }
}
