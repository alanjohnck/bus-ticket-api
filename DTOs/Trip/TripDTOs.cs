namespace BusBookingSystem.API.DTOs.Trip
{
    // GET /api/trips/:tripId
    public class TripDetailsDto
    {
        public Guid TripId { get; set; }
        public Guid ScheduleId { get; set; }
        public DateTime TripDate { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public int AvailableSeats { get; set; }
        public BusInfoDto Bus { get; set; } = new();
        public RouteInfoDto Route { get; set; } = new();
    }

    public class BusInfoDto
    {
        public Guid BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string BusType { get; set; } = string.Empty;
        public string BusCategory { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public List<string> Amenities { get; set; } = new();
    }

    public class RouteInfoDto
    {
        public Guid RouteId { get; set; }
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
    }

    // GET /api/trips/:tripId/availability
    public class SeatAvailabilityDto
    {
        public Guid TripId { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public int BookedSeats { get; set; }
        public List<AvailableSeatDto> Seats { get; set; } = new();
    }

    public class AvailableSeatDto
    {
        public string SeatNumber { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public string Deck { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public decimal Fare { get; set; }
    }

    // GET /api/trips/:tripId/fare
    public class FareDetailsDto
    {
        public Guid TripId { get; set; }
        public decimal BaseFare { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal TotalFare { get; set; }
    }

    // GET /api/trips/:tripId/stops
    public class TripStopsDto
    {
        public Guid TripId { get; set; }
        public List<StopDto> BoardingPoints { get; set; } = new();
        public List<StopDto> DroppingPoints { get; set; } = new();
    }

    public class StopDto
    {
        public Guid StopId { get; set; }
        public string StopName { get; set; } = string.Empty;
        public int StopOrder { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public TimeSpan DepartureTime { get; set; }
    }
}
