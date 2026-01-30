using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.Models
{
    public class Review
    {
        [Key]
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        public Guid TripId { get; set; }
        public Guid OperatorId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}