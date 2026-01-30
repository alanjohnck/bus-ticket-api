using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Bus;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BusesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/buses/{busId}
        [HttpGet("{busId:guid}")]
        public async Task<ActionResult<ApiResponse<BusDetailsDto>>> GetBusDetails(Guid busId)
        {
            var bus = await _context.Buses
                .Include(b => b.Operator)
                .FirstOrDefaultAsync(b => b.BusId == busId);

            if (bus == null)
                return NotFound(ApiResponse<BusDetailsDto>.FailureResponse("Bus not found"));

            var response = new BusDetailsDto
            {
                BusId = bus.BusId,
                BusNumber = bus.BusNumber,
                BusType = bus.BusType.ToString(),
                BusCategory = bus.BusCategory.ToString(),
                TotalSeats = bus.TotalSeats,
                RegistrationNumber = bus.RegistrationNumber,
                IsActive = bus.IsActive,
                Operator = new OperatorSummaryDto
                {
                    OperatorId = bus.Operator.OperatorId,
                    CompanyName = bus.Operator.CompanyName,
                    Rating = bus.Operator.Rating
                },
                Amenities = ParseAmenities(bus.AmenitiesJson)
            };

            return Ok(ApiResponse<BusDetailsDto>.SuccessResponse(response));
        }

        // GET: api/buses/{busId}/amenities
        [HttpGet("{busId:guid}/amenities")]
        public async Task<ActionResult<ApiResponse<BusAmenitiesDto>>> GetBusAmenities(Guid busId)
        {
            var bus = await _context.Buses.FindAsync(busId);

            if (bus == null)
                return NotFound(ApiResponse<BusAmenitiesDto>.FailureResponse("Bus not found"));

            var response = new BusAmenitiesDto
            {
                BusId = bus.BusId,
                Amenities = ParseAmenities(bus.AmenitiesJson)
            };

            return Ok(ApiResponse<BusAmenitiesDto>.SuccessResponse(response));
        }

        // GET: api/buses/{busId}/reviews
        [HttpGet("{busId:guid}/reviews")]
        public async Task<ActionResult<ApiResponse<BusReviewsResponseDto>>> GetBusReviews(Guid busId, [FromQuery] PaginationQuery pagination)
        {
            var bus = await _context.Buses
                .Include(b => b.Operator)
                .FirstOrDefaultAsync(b => b.BusId == busId);

            if (bus == null)
                return NotFound(ApiResponse<BusReviewsResponseDto>.FailureResponse("Bus not found"));

            var reviewsQuery = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.OperatorId == bus.OperatorId);

            var totalReviews = await reviewsQuery.CountAsync();
            var averageRating = totalReviews > 0 ? await reviewsQuery.AverageAsync(r => r.Rating) : 0;

            var reviews = await reviewsQuery
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(r => new BusReviewDto
                {
                    ReviewId = r.ReviewId,
                    UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Anonymous",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            var response = new BusReviewsResponseDto
            {
                BusId = busId,
                AverageRating = (decimal)averageRating,
                TotalReviews = totalReviews,
                Reviews = reviews
            };

            return Ok(ApiResponse<BusReviewsResponseDto>.SuccessResponse(response));
        }

        // GET: api/buses/{busId}/seat-layout
        [HttpGet("{busId:guid}/seat-layout")]
        public async Task<ActionResult<ApiResponse<BusSeatLayoutResponseDto>>> GetBusSeatLayout(Guid busId)
        {
            var bus = await _context.Buses
                .Include(b => b.SeatLayouts)
                .FirstOrDefaultAsync(b => b.BusId == busId);

            if (bus == null)
                return NotFound(ApiResponse<BusSeatLayoutResponseDto>.FailureResponse("Bus not found"));

            var lowerDeck = bus.SeatLayouts
                .Where(s => s.Deck == DeckType.Lower)
                .Select(s => new SeatLayoutDto
                {
                    LayoutId = s.LayoutId,
                    SeatNumber = s.SeatNumber,
                    SeatType = s.SeatType.ToString(),
                    Deck = s.Deck.ToString(),
                    PositionX = s.PositionX,
                    PositionY = s.PositionY,
                    IsAvailable = s.IsAvailable,
                    IsBooked = false // Will be updated based on trip context
                })
                .OrderBy(s => s.PositionY)
                .ThenBy(s => s.PositionX)
                .ToList();

            var upperDeck = bus.SeatLayouts
                .Where(s => s.Deck == DeckType.Upper)
                .Select(s => new SeatLayoutDto
                {
                    LayoutId = s.LayoutId,
                    SeatNumber = s.SeatNumber,
                    SeatType = s.SeatType.ToString(),
                    Deck = s.Deck.ToString(),
                    PositionX = s.PositionX,
                    PositionY = s.PositionY,
                    IsAvailable = s.IsAvailable,
                    IsBooked = false
                })
                .OrderBy(s => s.PositionY)
                .ThenBy(s => s.PositionX)
                .ToList();

            var response = new BusSeatLayoutResponseDto
            {
                BusId = bus.BusId,
                TotalSeats = bus.TotalSeats,
                LowerDeck = lowerDeck,
                UpperDeck = upperDeck
            };

            return Ok(ApiResponse<BusSeatLayoutResponseDto>.SuccessResponse(response));
        }

        // Helper method
        private static List<string> ParseAmenities(string? amenitiesJson)
        {
            if (string.IsNullOrEmpty(amenitiesJson))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(amenitiesJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
