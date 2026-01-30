using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBookingSystem.API.Models
{
    public class Cancellation
    {
        [Key]
        public Guid CancellationId { get; set; }

        [ForeignKey("Booking")]
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }

        [ForeignKey("User")]
        public Guid CancelledById { get; set; }
        public User CancelledBy { get; set; }

        public string CancellationReason { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CancellationCharges { get; set; }

        public DateTime CancellationDate { get; set; } = DateTime.UtcNow;
        public RefundStatus RefundStatus { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}