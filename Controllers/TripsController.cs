using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.DTOs.Trip;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TripsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/trips/{tripId}
        [HttpGet("{tripId:guid}")]
        public async Task<ActionResult<ApiResponse<TripDetailsDto>>> GetTripDetails(Guid tripId)
        {
            var trip = await _context.Trips
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Bus)
                        .ThenInclude(b => b.Operator)
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Route)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip == null)
                return NotFound(ApiResponse<TripDetailsDto>.FailureResponse("Trip not found"));

            var amenities = string.IsNullOrEmpty(trip.Schedule.Bus.AmenitiesJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(trip.Schedule.Bus.AmenitiesJson) ?? new List<string>();

            var response = new TripDetailsDto
            {
                TripId = trip.TripId,
                ScheduleId = trip.ScheduleId,
                TripDate = trip.TripDate,
                DepartureDateTime = trip.DepartureDateTime,
                ArrivalDateTime = trip.ArrivalDateTime,
                CurrentStatus = trip.CurrentStatus.ToString(),
                AvailableSeats = trip.AvailableSeats,
                Bus = new BusInfoDto
                {
                    BusId = trip.Schedule.BusId,
                    BusNumber = trip.Schedule.Bus.BusNumber,
                    BusType = trip.Schedule.Bus.BusType.ToString(),
                    BusCategory = trip.Schedule.Bus.BusCategory.ToString(),
                    OperatorName = trip.Schedule.Bus.Operator.CompanyName,
                    Amenities = amenities
                },
                Route = new RouteInfoDto
                {
                    RouteId = trip.Schedule.RouteId,
                    SourceCity = trip.Schedule.Route.SourceCity,
                    DestinationCity = trip.Schedule.Route.DestinationCity,
                    DistanceKm = trip.Schedule.Route.DistanceKm
                }
            };

            return Ok(ApiResponse<TripDetailsDto>.SuccessResponse(response));
        }

        // GET: api/trips/{tripId}/availability
        [HttpGet("{tripId:guid}/availability")]
        public async Task<ActionResult<ApiResponse<SeatAvailabilityDto>>> GetSeatAvailability(Guid tripId)
        {
            var trip = await _context.Trips
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Bus)
                        .ThenInclude(b => b.SeatLayouts)
                .Include(t => t.Bookings)
                    .ThenInclude(b => b.BookedSeats)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip == null)
                return NotFound(ApiResponse<SeatAvailabilityDto>.FailureResponse("Trip not found"));

            // Get all booked seat numbers for confirmed bookings
            var bookedSeatNumbers = trip.Bookings
                .Where(b => b.BookingStatus == BookingStatus.Confirmed)
                .SelectMany(b => b.BookedSeats)
                .Select(bs => bs.SeatNumber)
                .ToHashSet();

            var seats = trip.Schedule.Bus.SeatLayouts
                .Select(s => new AvailableSeatDto
                {
                    SeatNumber = s.SeatNumber,
                    SeatType = s.SeatType.ToString(),
                    Deck = s.Deck.ToString(),
                    IsAvailable = s.IsAvailable && !bookedSeatNumbers.Contains(s.SeatNumber),
                    Fare = trip.Schedule.BaseFare
                })
                .ToList();

            var response = new SeatAvailabilityDto
            {
                TripId = tripId,
                TotalSeats = trip.Schedule.Bus.TotalSeats,
                AvailableSeats = seats.Count(s => s.IsAvailable),
                BookedSeats = bookedSeatNumbers.Count,
                Seats = seats
            };

            return Ok(ApiResponse<SeatAvailabilityDto>.SuccessResponse(response));
        }

        // GET: api/trips/{tripId}/fare
        [HttpGet("{tripId:guid}/fare")]
        public async Task<ActionResult<ApiResponse<FareDetailsDto>>> GetFareDetails(Guid tripId)
        {
            var trip = await _context.Trips
                .Include(t => t.Schedule)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip == null)
                return NotFound(ApiResponse<FareDetailsDto>.FailureResponse("Trip not found"));

            var baseFare = trip.Schedule.BaseFare;
            var taxAmount = baseFare * 0.05m; // 5% tax
            var serviceCharge = 25m; // Fixed service charge

            var response = new FareDetailsDto
            {
                TripId = tripId,
                BaseFare = baseFare,
                TaxAmount = taxAmount,
                ServiceCharge = serviceCharge,
                TotalFare = baseFare + taxAmount + serviceCharge
            };

            return Ok(ApiResponse<FareDetailsDto>.SuccessResponse(response));
        }

        // GET: api/trips/{tripId}/stops
        [HttpGet("{tripId:guid}/stops")]
        public async Task<ActionResult<ApiResponse<TripStopsDto>>> GetTripStops(Guid tripId)
        {
            var trip = await _context.Trips
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.Stops)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip == null)
                return NotFound(ApiResponse<TripStopsDto>.FailureResponse("Trip not found"));

            var stops = trip.Schedule.Route.Stops
                .OrderBy(s => s.StopOrder)
                .ToList();

            // For simplicity, first half are boarding points, second half are dropping points
            var midpoint = stops.Count / 2;
            if (midpoint == 0) midpoint = 1;

            var boardingPoints = stops.Take(midpoint)
                .Select(s => new StopDto
                {
                    StopId = s.StopId,
                    StopName = s.StopName,
                    StopOrder = s.StopOrder,
                    ArrivalTime = s.ArrivalTimeOffset,
                    DepartureTime = s.DepartureTimeOffset
                })
                .ToList();

            var droppingPoints = stops.Skip(midpoint)
                .Select(s => new StopDto
                {
                    StopId = s.StopId,
                    StopName = s.StopName,
                    StopOrder = s.StopOrder,
                    ArrivalTime = s.ArrivalTimeOffset,
                    DepartureTime = s.DepartureTimeOffset
                })
                .ToList();

            var response = new TripStopsDto
            {
                TripId = tripId,
                BoardingPoints = boardingPoints,
                DroppingPoints = droppingPoints
            };

            return Ok(ApiResponse<TripStopsDto>.SuccessResponse(response));
        }
    }
}
