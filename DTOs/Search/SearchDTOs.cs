using BusBookingSystem.API.Models;

namespace BusBookingSystem.API.DTOs.Search
{
    // GET /api/search/buses - Query parameters
    public class BusSearchQueryDto
    {
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Passengers { get; set; } = 1;
        public BusType? BusType { get; set; }
        public BusCategory? Category { get; set; }
    }

    public class BusSearchResultDto
    {
        public Guid TripId { get; set; }
        public Guid BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string BusType { get; set; } = string.Empty;
        public string BusCategory { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Duration { get; set; } = string.Empty;
        public decimal BaseFare { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Rating { get; set; }
        public List<string> Amenities { get; set; } = new();
    }

    // GET /api/search/routes
    public class RouteDto
    {
        public Guid RouteId { get; set; }
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal EstimatedDurationHours { get; set; }
    }

    // GET /api/search/cities
    public class CityDto
    {
        public string CityName { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }

    // GET /api/search/popular-routes
    public class PopularRouteDto
    {
        public Guid RouteId { get; set; }
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public decimal StartingFare { get; set; }
        public int TotalTrips { get; set; }
    }

    // GET /api/search/autocomplete
    public class AutocompleteResultDto
    {
        public string CityName { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
    }
}
