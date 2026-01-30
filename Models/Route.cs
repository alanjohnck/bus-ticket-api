using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.Models
{
    public class Route
    {
        [Key]
        public Guid RouteId { get; set; }
        public string SourceCity { get; set; }
        public string DestinationCity { get; set; }
        public decimal DistanceKm { get; set; }
        public decimal EstimatedDurationHours { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Stop> Stops { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
    }
}