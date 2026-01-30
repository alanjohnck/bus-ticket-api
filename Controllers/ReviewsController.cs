using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.DTOs.Review;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/reviews
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CreateReviewResponseDto>>> CreateReview([FromBody] CreateReviewRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CreateReviewResponseDto>.FailureResponse("Invalid input"));

            var userId = GetCurrentUserId();

            // Validate trip
            var trip = await _context.Trips.FindAsync(request.TripId);
            if (trip == null)
                return NotFound(ApiResponse<CreateReviewResponseDto>.FailureResponse("Trip not found"));

            // Validate operator
            var operatorEntity = await _context.BusOperators.FindAsync(request.OperatorId);
            if (operatorEntity == null)
                return NotFound(ApiResponse<CreateReviewResponseDto>.FailureResponse("Operator not found"));

            // Check for existing review
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.TripId == request.TripId);

            if (existingReview != null)
                return BadRequest(ApiResponse<CreateReviewResponseDto>.FailureResponse("You have already reviewed this trip"));

            var review = new Review
            {
                ReviewId = Guid.NewGuid(),
                UserId = userId,
                TripId = request.TripId,
                OperatorId = request.OperatorId,
                Rating = request.Rating,
                Comment = request.Comment ?? "",
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);

            // Update operator rating
            await UpdateOperatorRating(request.OperatorId);

            await _context.SaveChangesAsync();

            var response = new CreateReviewResponseDto
            {
                ReviewId = review.ReviewId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                Message = "Review submitted successfully"
            };

            return CreatedAtAction(nameof(GetReviewDetails), new { reviewId = review.ReviewId },
                ApiResponse<CreateReviewResponseDto>.SuccessResponse(response, "Review created"));
        }

        // GET: api/reviews/trip/{tripId}
        [HttpGet("trip/{tripId:guid}")]
        public async Task<ActionResult<ApiResponse<ReviewListResponseDto>>> GetTripReviews(Guid tripId, [FromQuery] PaginationQuery pagination)
        {
            var reviewsQuery = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.TripId == tripId);

            var totalReviews = await reviewsQuery.CountAsync();
            var averageRating = totalReviews > 0 ? await reviewsQuery.AverageAsync(r => r.Rating) : 0;

            var reviews = await reviewsQuery
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(r => new ReviewListItemDto
                {
                    ReviewId = r.ReviewId,
                    UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName[0]}." : "Anonymous",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            var response = new ReviewListResponseDto
            {
                AverageRating = (decimal)averageRating,
                TotalReviews = totalReviews,
                Reviews = reviews
            };

            return Ok(ApiResponse<ReviewListResponseDto>.SuccessResponse(response));
        }

        // GET: api/reviews/operator/{operatorId}
        [HttpGet("operator/{operatorId:guid}")]
        public async Task<ActionResult<ApiResponse<ReviewListResponseDto>>> GetOperatorReviews(Guid operatorId, [FromQuery] PaginationQuery pagination)
        {
            var reviewsQuery = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.OperatorId == operatorId);

            var totalReviews = await reviewsQuery.CountAsync();
            var averageRating = totalReviews > 0 ? await reviewsQuery.AverageAsync(r => r.Rating) : 0;

            var reviews = await reviewsQuery
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(r => new ReviewListItemDto
                {
                    ReviewId = r.ReviewId,
                    UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName[0]}." : "Anonymous",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            var response = new ReviewListResponseDto
            {
                AverageRating = (decimal)averageRating,
                TotalReviews = totalReviews,
                Reviews = reviews
            };

            return Ok(ApiResponse<ReviewListResponseDto>.SuccessResponse(response));
        }

        // GET: api/reviews/{reviewId}
        [HttpGet("{reviewId:guid}")]
        public async Task<ActionResult<ApiResponse<ReviewDetailsDto>>> GetReviewDetails(Guid reviewId)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Operator)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

            if (review == null)
                return NotFound(ApiResponse<ReviewDetailsDto>.FailureResponse("Review not found"));

            var response = new ReviewDetailsDto
            {
                ReviewId = review.ReviewId,
                UserId = review.UserId,
                UserName = review.User != null ? $"{review.User.FirstName} {review.User.LastName}" : "Unknown",
                TripId = review.TripId,
                OperatorId = review.OperatorId,
                OperatorName = review.Operator?.CompanyName ?? "Unknown",
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };

            return Ok(ApiResponse<ReviewDetailsDto>.SuccessResponse(response));
        }

        // PUT: api/reviews/{reviewId}
        [HttpPut("{reviewId:guid}")]
        public async Task<ActionResult<ApiResponse<ReviewDetailsDto>>> UpdateReview(Guid reviewId, [FromBody] UpdateReviewRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ReviewDetailsDto>.FailureResponse("Invalid input"));

            var userId = GetCurrentUserId();

            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
                return NotFound(ApiResponse<ReviewDetailsDto>.FailureResponse("Review not found"));

            if (review.UserId != userId)
                return Forbid();

            review.Rating = request.Rating;
            review.Comment = request.Comment ?? "";

            // Update operator rating
            await UpdateOperatorRating(review.OperatorId);

            await _context.SaveChangesAsync();

            return await GetReviewDetails(reviewId);
        }

        // DELETE: api/reviews/{reviewId}
        [HttpDelete("{reviewId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> DeleteReview(Guid reviewId)
        {
            var userId = GetCurrentUserId();

            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Review not found"));

            if (review.UserId != userId)
                return Forbid();

            var operatorId = review.OperatorId;

            _context.Reviews.Remove(review);

            // Update operator rating
            await UpdateOperatorRating(operatorId);

            await _context.SaveChangesAsync();

            var response = new MessageResponse
            {
                Success = true,
                Message = "Review deleted successfully"
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // Helper methods
        private Guid GetCurrentUserId()
        {
            // TODO: Implement proper JWT authentication
            return Guid.Empty;
        }

        private async Task UpdateOperatorRating(Guid operatorId)
        {
            var operatorEntity = await _context.BusOperators.FindAsync(operatorId);
            if (operatorEntity == null) return;

            var reviews = await _context.Reviews
                .Where(r => r.OperatorId == operatorId)
                .ToListAsync();

            if (reviews.Any())
            {
                operatorEntity.Rating = (decimal)reviews.Average(r => r.Rating);
            }
            else
            {
                operatorEntity.Rating = 0;
            }
        }
    }
}
