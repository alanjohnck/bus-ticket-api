using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.DTOs.Offer;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OffersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OffersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/offers
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<OfferListItemDto>>>> GetAllOffers()
        {
            var offers = await _context.Offers
                .Where(o => o.IsActive && o.ValidTo >= DateTime.UtcNow)
                .OrderBy(o => o.ValidTo)
                .Select(o => new OfferListItemDto
                {
                    OfferId = o.OfferId,
                    OfferCode = o.OfferCode,
                    Description = o.Description,
                    DiscountType = o.DiscountType.ToString(),
                    DiscountValue = o.DiscountValue,
                    MinBookingAmount = o.MinBookingAmount,
                    MaxDiscount = o.MaxDiscount,
                    ValidFrom = o.ValidFrom,
                    ValidTo = o.ValidTo,
                    IsActive = o.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<List<OfferListItemDto>>.SuccessResponse(offers));
        }

        // GET: api/offers/{offerId}
        [HttpGet("{offerId:guid}")]
        public async Task<ActionResult<ApiResponse<OfferDetailsDto>>> GetOfferDetails(Guid offerId)
        {
            var offer = await _context.Offers.FindAsync(offerId);

            if (offer == null)
                return NotFound(ApiResponse<OfferDetailsDto>.FailureResponse("Offer not found"));

            var response = new OfferDetailsDto
            {
                OfferId = offer.OfferId,
                OfferCode = offer.OfferCode,
                Description = offer.Description,
                DiscountType = offer.DiscountType.ToString(),
                DiscountValue = offer.DiscountValue,
                MinBookingAmount = offer.MinBookingAmount,
                MaxDiscount = offer.MaxDiscount,
                ValidFrom = offer.ValidFrom,
                ValidTo = offer.ValidTo,
                UsageLimit = offer.UsageLimit,
                TimesUsed = offer.TimesUsed,
                IsActive = offer.IsActive
            };

            return Ok(ApiResponse<OfferDetailsDto>.SuccessResponse(response));
        }

        // POST: api/offers/validate
        [HttpPost("validate")]
        public async Task<ActionResult<ApiResponse<ValidateOfferResponseDto>>> ValidateOffer([FromBody] ValidateOfferRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ValidateOfferResponseDto>.FailureResponse("Invalid input"));

            var offer = await _context.Offers
                .FirstOrDefaultAsync(o => o.OfferCode == request.OfferCode);

            if (offer == null)
            {
                return Ok(ApiResponse<ValidateOfferResponseDto>.SuccessResponse(new ValidateOfferResponseDto
                {
                    IsValid = false,
                    OfferCode = request.OfferCode,
                    Message = "Invalid offer code"
                }));
            }

            // Check if offer is active
            if (!offer.IsActive)
            {
                return Ok(ApiResponse<ValidateOfferResponseDto>.SuccessResponse(new ValidateOfferResponseDto
                {
                    IsValid = false,
                    OfferCode = request.OfferCode,
                    Message = "Offer is not active"
                }));
            }

            // Check validity period
            if (offer.ValidFrom > DateTime.UtcNow || offer.ValidTo < DateTime.UtcNow)
            {
                return Ok(ApiResponse<ValidateOfferResponseDto>.SuccessResponse(new ValidateOfferResponseDto
                {
                    IsValid = false,
                    OfferCode = request.OfferCode,
                    Message = "Offer has expired or not yet valid"
                }));
            }

            // Check usage limit
            if (offer.TimesUsed >= offer.UsageLimit)
            {
                return Ok(ApiResponse<ValidateOfferResponseDto>.SuccessResponse(new ValidateOfferResponseDto
                {
                    IsValid = false,
                    OfferCode = request.OfferCode,
                    Message = "Offer usage limit reached"
                }));
            }

            // Check minimum booking amount
            if (request.BookingAmount < offer.MinBookingAmount)
            {
                return Ok(ApiResponse<ValidateOfferResponseDto>.SuccessResponse(new ValidateOfferResponseDto
                {
                    IsValid = false,
                    OfferCode = request.OfferCode,
                    Message = $"Minimum booking amount of Rs. {offer.MinBookingAmount} required"
                }));
            }

            // Calculate discount
            decimal discountAmount;
            if (offer.DiscountType == DiscountType.Percentage)
            {
                discountAmount = request.BookingAmount * (offer.DiscountValue / 100);
                if (discountAmount > offer.MaxDiscount)
                    discountAmount = offer.MaxDiscount;
            }
            else
            {
                discountAmount = offer.DiscountValue;
                if (discountAmount > offer.MaxDiscount)
                    discountAmount = offer.MaxDiscount;
            }

            var response = new ValidateOfferResponseDto
            {
                IsValid = true,
                OfferCode = offer.OfferCode,
                DiscountAmount = discountAmount,
                FinalAmount = request.BookingAmount - discountAmount,
                Message = $"Offer applied! You save Rs. {discountAmount:F2}"
            };

            return Ok(ApiResponse<ValidateOfferResponseDto>.SuccessResponse(response));
        }

        // GET: api/offers/applicable
        [HttpGet("applicable")]
        public async Task<ActionResult<ApiResponse<List<ApplicableOfferDto>>>> GetApplicableOffers([FromQuery] decimal? bookingAmount)
        {
            var query = _context.Offers
                .Where(o => o.IsActive
                         && o.ValidFrom <= DateTime.UtcNow
                         && o.ValidTo >= DateTime.UtcNow
                         && o.TimesUsed < o.UsageLimit);

            if (bookingAmount.HasValue)
            {
                query = query.Where(o => o.MinBookingAmount <= bookingAmount.Value);
            }

            var offers = await query
                .OrderByDescending(o => o.DiscountValue)
                .Select(o => new ApplicableOfferDto
                {
                    OfferId = o.OfferId,
                    OfferCode = o.OfferCode,
                    Description = o.Description,
                    DiscountType = o.DiscountType.ToString(),
                    DiscountValue = o.DiscountValue,
                    MaxDiscount = o.MaxDiscount
                })
                .ToListAsync();

            return Ok(ApiResponse<List<ApplicableOfferDto>>.SuccessResponse(offers));
        }
    }
}
