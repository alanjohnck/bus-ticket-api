using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.DTOs.Review
{
    // POST /api/reviews
    public class CreateReviewRequestDto
    {
        [Required]
        public Guid TripId { get; set; }

        [Required]
        public Guid OperatorId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }

    public class CreateReviewResponseDto
    {
        public Guid ReviewId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // GET /api/reviews/:reviewId
    public class ReviewDetailsDto
    {
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid TripId { get; set; }
        public Guid OperatorId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // PUT /api/reviews/:reviewId
    public class UpdateReviewRequestDto
    {
        [Required, Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }

    // GET /api/reviews/trip/:tripId or operator/:operatorId
    public class ReviewListItemDto
    {
        public Guid ReviewId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ReviewListResponseDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ReviewListItemDto> Reviews { get; set; } = new();
    }
}
