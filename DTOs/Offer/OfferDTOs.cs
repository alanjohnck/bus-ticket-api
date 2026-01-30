using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.DTOs.Offer
{
    // GET /api/offers
    public class OfferListItemDto
    {
        public Guid OfferId { get; set; }
        public string OfferCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal MinBookingAmount { get; set; }
        public decimal MaxDiscount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; }
    }

    // GET /api/offers/:offerId
    public class OfferDetailsDto
    {
        public Guid OfferId { get; set; }
        public string OfferCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal MinBookingAmount { get; set; }
        public decimal MaxDiscount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int UsageLimit { get; set; }
        public int TimesUsed { get; set; }
        public bool IsActive { get; set; }
    }

    // POST /api/offers/validate
    public class ValidateOfferRequestDto
    {
        [Required]
        public string OfferCode { get; set; } = string.Empty;

        [Required, Range(0.01, double.MaxValue)]
        public decimal BookingAmount { get; set; }
    }

    public class ValidateOfferResponseDto
    {
        public bool IsValid { get; set; }
        public string OfferCode { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // GET /api/offers/applicable
    public class ApplicableOfferDto
    {
        public Guid OfferId { get; set; }
        public string OfferCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal MaxDiscount { get; set; }
    }
}
