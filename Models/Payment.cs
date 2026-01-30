using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.Models
{
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }

        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }
}