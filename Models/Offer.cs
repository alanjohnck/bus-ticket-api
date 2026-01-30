using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBookingSystem.API.Models
{
    public class Offer
    {
        [Key]
        public Guid OfferId { get; set; }

        [Required]
        public string OfferCode { get; set; } // Unique

        public string Description { get; set; }
        public DiscountType DiscountType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinBookingAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxDiscount { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int UsageLimit { get; set; }
        public int TimesUsed { get; set; }
        public bool IsActive { get; set; }
    }
}