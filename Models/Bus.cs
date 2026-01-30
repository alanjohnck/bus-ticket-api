using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.Models
{
    public class Bus
    {
        [Key]
        public Guid BusId { get; set; }
        public Guid OperatorId { get; set; }
        public BusOperator Operator { get; set; }

        public string BusNumber { get; set; }
        public BusType BusType { get; set; }
        public BusCategory BusCategory { get; set; }
        public int TotalSeats { get; set; }
        public string AmenitiesJson { get; set; } // Stored as JSON string [cite: 255]
        public string RegistrationNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SeatLayout> SeatLayouts { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
    }
}