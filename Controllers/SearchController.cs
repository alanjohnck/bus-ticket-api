using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.DTOs.Search;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/search/buses
        [HttpGet("buses")]
        public async Task<ActionResult<ApiResponse<List<BusSearchResultDto>>>> SearchBuses([FromQuery] BusSearchQueryDto query)
        {
            if (string.IsNullOrEmpty(query.Source) || string.IsNullOrEmpty(query.Destination))
                return BadRequest(ApiResponse<List<BusSearchResultDto>>.FailureResponse("Source and destination are required"));

            var tripsQuery = _context.Trips
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Bus)
                        .ThenInclude(b => b.Operator)
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Route)
                .Where(t => t.Schedule.Route.SourceCity.ToLower() == query.Source.ToLower()
                         && t.Schedule.Route.DestinationCity.ToLower() == query.Destination.ToLower()
                         && t.TripDate.Date == query.Date.Date
                         && t.CurrentStatus == TripStatus.Scheduled
                         && t.AvailableSeats >= query.Passengers
                         && t.Schedule.Bus.IsActive
                         && t.Schedule.IsActive);

            // Apply optional filters
            if (query.BusType.HasValue)
            {
                tripsQuery = tripsQuery.Where(t => t.Schedule.Bus.BusType == query.BusType.Value);
            }

            if (query.Category.HasValue)
            {
                tripsQuery = tripsQuery.Where(t => t.Schedule.Bus.BusCategory == query.Category.Value);
            }

            var tripsData = await tripsQuery
                .OrderBy(t => t.DepartureDateTime)
                .ToListAsync();

            var trips = tripsData.Select(t => new BusSearchResultDto
            {
                TripId = t.TripId,
                BusId = t.Schedule.BusId,
                BusNumber = t.Schedule.Bus.BusNumber,
                OperatorName = t.Schedule.Bus.Operator.CompanyName,
                BusType = t.Schedule.Bus.BusType.ToString(),
                BusCategory = t.Schedule.Bus.BusCategory.ToString(),
                DepartureTime = t.DepartureDateTime,
                ArrivalTime = t.ArrivalDateTime,
                Duration = FormatDuration(t.ArrivalDateTime - t.DepartureDateTime),
                BaseFare = t.Schedule.BaseFare,
                AvailableSeats = t.AvailableSeats,
                Rating = t.Schedule.Bus.Operator.Rating,
                Amenities = ParseAmenities(t.Schedule.Bus.AmenitiesJson)
            }).ToList();

            return Ok(ApiResponse<List<BusSearchResultDto>>.SuccessResponse(trips, $"Found {trips.Count} buses"));
        }

        // GET: api/search/routes
        [HttpGet("routes")]
        public async Task<ActionResult<ApiResponse<List<RouteDto>>>> GetAllRoutes()
        {
            var routes = await _context.Routes
                .Select(r => new RouteDto
                {
                    RouteId = r.RouteId,
                    SourceCity = r.SourceCity,
                    DestinationCity = r.DestinationCity,
                    DistanceKm = r.DistanceKm,
                    EstimatedDurationHours = r.EstimatedDurationHours
                })
                .ToListAsync();

            return Ok(ApiResponse<List<RouteDto>>.SuccessResponse(routes));
        }

        // GET: api/search/cities
        [HttpGet("cities")]
        public async Task<ActionResult<ApiResponse<List<CityDto>>>> GetCities()
        {
            var sourceCities = await _context.Routes
                .Select(r => r.SourceCity)
                .Distinct()
                .ToListAsync();

            var destCities = await _context.Routes
                .Select(r => r.DestinationCity)
                .Distinct()
                .ToListAsync();

            var allCities = sourceCities.Union(destCities)
                .Distinct()
                .OrderBy(c => c)
                .Select(c => new CityDto
                {
                    CityName = c,
                    State = "" // Can be populated if you have state data
                })
                .ToList();

            return Ok(ApiResponse<List<CityDto>>.SuccessResponse(allCities));
        }

        // GET: api/search/popular-routes
        [HttpGet("popular-routes")]
        public async Task<ActionResult<ApiResponse<List<PopularRouteDto>>>> GetPopularRoutes()
        {
            var routesData = await _context.Routes
                .Include(r => r.Schedules)
                    .ThenInclude(s => s.Trips)
                .ToListAsync();

            var popularRoutes = routesData
                .Select(r => new PopularRouteDto
                {
                    RouteId = r.RouteId,
                    SourceCity = r.SourceCity,
                    DestinationCity = r.DestinationCity,
                    StartingFare = r.Schedules.Any() ? r.Schedules.Min(s => s.BaseFare) : 0,
                    TotalTrips = r.Schedules.SelectMany(s => s.Trips).Count()
                })
                .OrderByDescending(r => r.TotalTrips)
                .Take(10)
                .ToList();

            return Ok(ApiResponse<List<PopularRouteDto>>.SuccessResponse(popularRoutes));
        }

        // GET: api/search/autocomplete
        [HttpGet("autocomplete")]
        public async Task<ActionResult<ApiResponse<List<AutocompleteResultDto>>>> Autocomplete([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Ok(ApiResponse<List<AutocompleteResultDto>>.SuccessResponse(new List<AutocompleteResultDto>()));

            var searchTerm = query.ToLower();

            var sourceCities = await _context.Routes
                .Where(r => r.SourceCity.ToLower().Contains(searchTerm))
                .Select(r => r.SourceCity)
                .Distinct()
                .ToListAsync();

            var destCities = await _context.Routes
                .Where(r => r.DestinationCity.ToLower().Contains(searchTerm))
                .Select(r => r.DestinationCity)
                .Distinct()
                .ToListAsync();

            var results = sourceCities.Union(destCities)
                .Distinct()
                .OrderBy(c => c)
                .Take(10)
                .Select(c => new AutocompleteResultDto
                {
                    CityName = c,
                    State = "",
                    DisplayText = c
                })
                .ToList();

            return Ok(ApiResponse<List<AutocompleteResultDto>>.SuccessResponse(results));
        }

        // Helper methods
        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
            {
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            }
            return $"{duration.Minutes}m";
        }

        private static List<string> ParseAmenities(string? amenitiesJson)
        {
            if (string.IsNullOrEmpty(amenitiesJson))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(amenitiesJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
