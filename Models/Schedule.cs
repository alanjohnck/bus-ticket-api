using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.Models
{
    public class Schedule
    {
        [Key]
        public Guid ScheduleId { get; set; }
        public Guid BusId { get; set; }
        public Bus Bus { get; set; }
        public Guid RouteId { get; set; }
        public Route Route { get; set; }

        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal BaseFare { get; set; }
        public string AvailableDatesJson { get; set; } // Simplified for array storage
        public bool IsActive { get; set; }

        // Navigation
        public ICollection<Trip> Trips { get; set; }
    }
}