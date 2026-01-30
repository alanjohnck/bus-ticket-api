using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBookingSystem.API.Models
{
    public class Review
    {
        [Key]
        public Guid ReviewId { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Trip")]
        public Guid TripId { get; set; }
        public Trip Trip { get; set; }

        [ForeignKey("Operator")]
        public Guid OperatorId { get; set; }
        public BusOperator Operator { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}