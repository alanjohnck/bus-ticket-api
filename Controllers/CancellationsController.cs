using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Cancellation;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CancellationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        // Cancellation policy slabs
        private static readonly List<(int HoursBefore, decimal ChargePercentage, string Description)> CancellationSlabs = new()
        {
            (48, 10, "More than 48 hours before departure - 10% cancellation charge"),
            (24, 25, "24-48 hours before departure - 25% cancellation charge"),
            (12, 50, "12-24 hours before departure - 50% cancellation charge"),
            (6, 75, "6-12 hours before departure - 75% cancellation charge"),
            (0, 100, "Less than 6 hours before departure - No refund")
        };

        public CancellationsController(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // POST: api/cancellations
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CreateCancellationResponseDto>>> CreateCancellation([FromBody] CreateCancellationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CreateCancellationResponseDto>.FailureResponse("Invalid input"));

            var booking = await _context.Bookings
                .Include(b => b.Trip)
                .Include(b => b.Cancellation)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId);

            if (booking == null)
                return NotFound(ApiResponse<CreateCancellationResponseDto>.FailureResponse("Booking not found"));

            if (booking.BookingStatus != BookingStatus.Confirmed)
                return BadRequest(ApiResponse<CreateCancellationResponseDto>.FailureResponse("Booking is not in confirmed status"));

            if (booking.Cancellation != null)
                return BadRequest(ApiResponse<CreateCancellationResponseDto>.FailureResponse("Booking already cancelled"));

            if (booking.Trip.DepartureDateTime <= DateTime.UtcNow)
                return BadRequest(ApiResponse<CreateCancellationResponseDto>.FailureResponse("Cannot cancel after departure"));

            // Calculate refund
            var (refundAmount, cancellationCharges, appliedSlab) = CalculateRefund(booking.TotalFare, booking.Trip.DepartureDateTime);

            var userId = await GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse<CreateCancellationResponseDto>.FailureResponse("Invalid or missing token. Please login first."));

            var cancellation = new Cancellation
            {
                CancellationId = Guid.NewGuid(),
                BookingId = booking.BookingId,
                CancelledById = userId.Value,
                CancellationReason = request.Reason ?? "Customer requested cancellation",
                RefundAmount = refundAmount,
                CancellationCharges = cancellationCharges,
                CancellationDate = DateTime.UtcNow,
                RefundStatus = RefundStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Cancellations.Add(cancellation);

            // Update booking status
            booking.BookingStatus = BookingStatus.Cancelled;

            // Restore available seats
            booking.Trip.AvailableSeats += booking.TotalSeats;

            // Update payment status if payment was completed
            if (booking.Payment != null && booking.Payment.PaymentStatus == PaymentStatus.Completed)
            {
                booking.Payment.PaymentStatus = PaymentStatus.Refunded;
            }

            await _context.SaveChangesAsync();

            var response = new CreateCancellationResponseDto
            {
                CancellationId = cancellation.CancellationId,
                BookingId = booking.BookingId,
                BookingReference = booking.BookingReference,
                RefundAmount = refundAmount,
                CancellationCharges = cancellationCharges,
                RefundStatus = cancellation.RefundStatus.ToString(),
                CancellationDate = cancellation.CancellationDate,
                Message = $"Booking cancelled successfully. {appliedSlab}. Refund of Rs. {refundAmount:F2} will be processed."
            };

            return Ok(ApiResponse<CreateCancellationResponseDto>.SuccessResponse(response, "Cancellation successful"));
        }

        // GET: api/cancellations/{cancellationId}
        [HttpGet("{cancellationId:guid}")]
        public async Task<ActionResult<ApiResponse<CancellationDetailsDto>>> GetCancellationDetails(Guid cancellationId)
        {
            var cancellation = await _context.Cancellations
                .Include(c => c.Booking)
                .Include(c => c.CancelledBy)
                .FirstOrDefaultAsync(c => c.CancellationId == cancellationId);

            if (cancellation == null)
                return NotFound(ApiResponse<CancellationDetailsDto>.FailureResponse("Cancellation not found"));

            var response = new CancellationDetailsDto
            {
                CancellationId = cancellation.CancellationId,
                BookingId = cancellation.BookingId,
                BookingReference = cancellation.Booking.BookingReference,
                CancellationReason = cancellation.CancellationReason,
                RefundAmount = cancellation.RefundAmount,
                CancellationCharges = cancellation.CancellationCharges,
                CancellationDate = cancellation.CancellationDate,
                RefundStatus = cancellation.RefundStatus.ToString(),
                CancelledBy = new CancelledByDto
                {
                    UserId = cancellation.CancelledBy.UserId,
                    Name = $"{cancellation.CancelledBy.FirstName} {cancellation.CancelledBy.LastName}",
                    Email = cancellation.CancelledBy.Email
                }
            };

            return Ok(ApiResponse<CancellationDetailsDto>.SuccessResponse(response));
        }

        // GET: api/cancellations/policy
        [HttpGet("policy")]
        public ActionResult<ApiResponse<CancellationPolicyDto>> GetCancellationPolicy()
        {
            var policy = new CancellationPolicyDto
            {
                PolicyDescription = "Cancellation charges are based on how far in advance you cancel before the scheduled departure time.",
                Slabs = CancellationSlabs.Select(s => new CancellationSlabDto
                {
                    HoursBeforeDeparture = s.HoursBefore,
                    CancellationChargePercentage = s.ChargePercentage,
                    Description = s.Description
                }).ToList()
            };

            return Ok(ApiResponse<CancellationPolicyDto>.SuccessResponse(policy));
        }

        // POST: api/cancellations/calculate
        [HttpPost("calculate")]
        public async Task<ActionResult<ApiResponse<CalculateRefundResponseDto>>> CalculateRefundAmount([FromBody] CalculateRefundRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CalculateRefundResponseDto>.FailureResponse("Invalid input"));

            var booking = await _context.Bookings
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId);

            if (booking == null)
                return NotFound(ApiResponse<CalculateRefundResponseDto>.FailureResponse("Booking not found"));

            if (booking.BookingStatus != BookingStatus.Confirmed)
                return BadRequest(ApiResponse<CalculateRefundResponseDto>.FailureResponse("Booking is not in confirmed status"));

            var (refundAmount, cancellationCharges, appliedSlab) = CalculateRefund(booking.TotalFare, booking.Trip.DepartureDateTime);

            var response = new CalculateRefundResponseDto
            {
                BookingId = booking.BookingId,
                TotalFare = booking.TotalFare,
                CancellationCharges = cancellationCharges,
                RefundAmount = refundAmount,
                AppliedSlab = appliedSlab
            };

            return Ok(ApiResponse<CalculateRefundResponseDto>.SuccessResponse(response));
        }

        // Helper methods
        private async Task<Guid?> GetCurrentUserId()
        {
            var token = Request.Headers["X-User-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
                return null;

            var userIdString = await _cache.GetStringAsync($"token:{token}");
            if (string.IsNullOrEmpty(userIdString))
                return null;

            if (Guid.TryParse(userIdString, out var userId))
                return userId;

            return null;
        }

        private static (decimal RefundAmount, decimal CancellationCharges, string AppliedSlab) CalculateRefund(decimal totalFare, DateTime departureTime)
        {
            var hoursBeforeDeparture = (departureTime - DateTime.UtcNow).TotalHours;

            foreach (var slab in CancellationSlabs)
            {
                if (hoursBeforeDeparture > slab.HoursBefore || (slab.HoursBefore == 0 && hoursBeforeDeparture > 0))
                {
                    var cancellationCharges = totalFare * (slab.ChargePercentage / 100);
                    var refundAmount = totalFare - cancellationCharges;
                    return (refundAmount, cancellationCharges, slab.Description);
                }
            }

            // No refund (less than minimum hours)
            return (0, totalFare, "No refund available - too close to departure time");
        }
    }
}
