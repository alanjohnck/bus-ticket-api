using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.Models
{
    public class Stop
    {
        [Key]
        public Guid StopId { get; set; }
        public Guid RouteId { get; set; }
        public Route Route { get; set; }

        public string StopName { get; set; }
        public int StopOrder { get; set; }
        public TimeSpan ArrivalTimeOffset { get; set; }
        public TimeSpan DepartureTimeOffset { get; set; }
    }
}