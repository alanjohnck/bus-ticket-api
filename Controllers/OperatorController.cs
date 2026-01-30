using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.DTOs.Operator;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperatorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OperatorController(AppDbContext context)
        {
            _context = context;
        }

        #region Dashboard & Profile

        // GET: api/operator/dashboard
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<OperatorDashboardDto>>> GetDashboard()
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<OperatorDashboardDto>.FailureResponse("Not authorized as operator"));

            var operatorEntity = await _context.BusOperators
                .Include(o => o.Buses)
                .FirstOrDefaultAsync(o => o.OperatorId == operatorId.Value);

            if (operatorEntity == null)
                return NotFound(ApiResponse<OperatorDashboardDto>.FailureResponse("Operator not found"));

            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var busIds = operatorEntity.Buses.Select(b => b.BusId).ToList();

            var todayTrips = await _context.Trips
                .Include(t => t.Schedule)
                .Where(t => busIds.Contains(t.Schedule.BusId) && t.TripDate.Date == today)
                .CountAsync();

            var todayBookings = await _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                .Where(b => busIds.Contains(b.Trip.Schedule.BusId)
                         && b.BookingDate.Date == today
                         && b.BookingStatus == BookingStatus.Confirmed)
                .ToListAsync();

            var monthlyBookings = await _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                .Where(b => busIds.Contains(b.Trip.Schedule.BusId)
                         && b.BookingDate >= monthStart
                         && b.BookingStatus == BookingStatus.Confirmed)
                .ToListAsync();

            var totalReviews = await _context.Reviews
                .Where(r => r.OperatorId == operatorId.Value)
                .CountAsync();

            var response = new OperatorDashboardDto
            {
                OperatorId = operatorEntity.OperatorId,
                CompanyName = operatorEntity.CompanyName,
                TotalBuses = operatorEntity.Buses.Count,
                ActiveBuses = operatorEntity.Buses.Count(b => b.IsActive),
                TotalRoutes = await _context.Schedules
                    .Where(s => busIds.Contains(s.BusId))
                    .Select(s => s.RouteId)
                    .Distinct()
                    .CountAsync(),
                TotalSchedules = await _context.Schedules
                    .Where(s => busIds.Contains(s.BusId) && s.IsActive)
                    .CountAsync(),
                TodayTrips = todayTrips,
                TotalBookingsToday = todayBookings.Count,
                TodayRevenue = todayBookings.Sum(b => b.TotalFare),
                MonthlyRevenue = monthlyBookings.Sum(b => b.TotalFare),
                AverageRating = operatorEntity.Rating,
                TotalReviews = totalReviews
            };

            return Ok(ApiResponse<OperatorDashboardDto>.SuccessResponse(response));
        }

        // GET: api/operator/profile
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<OperatorProfileDto>>> GetProfile()
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<OperatorProfileDto>.FailureResponse("Not authorized as operator"));

            var operatorEntity = await _context.BusOperators.FindAsync(operatorId.Value);

            if (operatorEntity == null)
                return NotFound(ApiResponse<OperatorProfileDto>.FailureResponse("Operator not found"));

            var response = new OperatorProfileDto
            {
                OperatorId = operatorEntity.OperatorId,
                CompanyName = operatorEntity.CompanyName,
                LicenseNumber = operatorEntity.LicenseNumber,
                ContactEmail = operatorEntity.ContactEmail,
                ContactPhone = operatorEntity.ContactPhone,
                Address = operatorEntity.Address,
                City = operatorEntity.City,
                State = operatorEntity.State,
                Rating = operatorEntity.Rating,
                IsApproved = operatorEntity.IsApproved,
                CreatedAt = operatorEntity.CreatedAt
            };

            return Ok(ApiResponse<OperatorProfileDto>.SuccessResponse(response));
        }

        // PUT: api/operator/profile
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<OperatorProfileDto>>> UpdateProfile([FromBody] UpdateOperatorProfileRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<OperatorProfileDto>.FailureResponse("Invalid input"));

            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<OperatorProfileDto>.FailureResponse("Not authorized as operator"));

            var operatorEntity = await _context.BusOperators.FindAsync(operatorId.Value);

            if (operatorEntity == null)
                return NotFound(ApiResponse<OperatorProfileDto>.FailureResponse("Operator not found"));

            operatorEntity.CompanyName = request.CompanyName;
            operatorEntity.ContactEmail = request.ContactEmail;
            operatorEntity.ContactPhone = request.ContactPhone;
            operatorEntity.Address = request.Address;
            operatorEntity.City = request.City;
            operatorEntity.State = request.State;

            await _context.SaveChangesAsync();

            return await GetProfile();
        }

        #endregion

        #region Fleet Management - Buses

        // GET: api/operator/buses
        [HttpGet("buses")]
        public async Task<ActionResult<ApiResponse<List<OperatorBusListItemDto>>>> GetBuses()
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<List<OperatorBusListItemDto>>.FailureResponse("Not authorized"));

            var busesData = await _context.Buses
                .Where(b => b.OperatorId == operatorId.Value)
                .ToListAsync();

            var buses = busesData.Select(b => new OperatorBusListItemDto
            {
                BusId = b.BusId,
                BusNumber = b.BusNumber,
                BusType = b.BusType.ToString(),
                BusCategory = b.BusCategory.ToString(),
                TotalSeats = b.TotalSeats,
                RegistrationNumber = b.RegistrationNumber,
                IsActive = b.IsActive,
                Amenities = ParseAmenities(b.AmenitiesJson)
            }).ToList();

            return Ok(ApiResponse<List<OperatorBusListItemDto>>.SuccessResponse(buses));
        }

        // POST: api/operator/buses
        [HttpPost("buses")]
        public async Task<ActionResult<ApiResponse<CreateBusResponseDto>>> CreateBus([FromBody] CreateBusRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CreateBusResponseDto>.FailureResponse("Invalid input"));

            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<CreateBusResponseDto>.FailureResponse("Not authorized"));

            // Check for duplicate bus number
            if (await _context.Buses.AnyAsync(b => b.BusNumber == request.BusNumber))
                return Conflict(ApiResponse<CreateBusResponseDto>.FailureResponse("Bus number already exists"));

            var bus = new Bus
            {
                BusId = Guid.NewGuid(),
                OperatorId = operatorId.Value,
                BusNumber = request.BusNumber,
                BusType = request.BusType,
                BusCategory = request.BusCategory,
                TotalSeats = request.TotalSeats,
                RegistrationNumber = request.RegistrationNumber,
                AmenitiesJson = JsonSerializer.Serialize(request.Amenities),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Buses.Add(bus);
            await _context.SaveChangesAsync();

            var response = new CreateBusResponseDto
            {
                BusId = bus.BusId,
                BusNumber = bus.BusNumber,
                Message = "Bus created successfully"
            };

            return CreatedAtAction(nameof(GetBusDetails), new { busId = bus.BusId },
                ApiResponse<CreateBusResponseDto>.SuccessResponse(response));
        }

        // GET: api/operator/buses/{busId}
        [HttpGet("buses/{busId:guid}")]
        public async Task<ActionResult<ApiResponse<OperatorBusListItemDto>>> GetBusDetails(Guid busId)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<OperatorBusListItemDto>.FailureResponse("Not authorized"));

            var bus = await _context.Buses
                .FirstOrDefaultAsync(b => b.BusId == busId && b.OperatorId == operatorId.Value);

            if (bus == null)
                return NotFound(ApiResponse<OperatorBusListItemDto>.FailureResponse("Bus not found"));

            var response = new OperatorBusListItemDto
            {
                BusId = bus.BusId,
                BusNumber = bus.BusNumber,
                BusType = bus.BusType.ToString(),
                BusCategory = bus.BusCategory.ToString(),
                TotalSeats = bus.TotalSeats,
                RegistrationNumber = bus.RegistrationNumber,
                IsActive = bus.IsActive,
                Amenities = ParseAmenities(bus.AmenitiesJson)
            };

            return Ok(ApiResponse<OperatorBusListItemDto>.SuccessResponse(response));
        }

        // PUT: api/operator/buses/{busId}
        [HttpPut("buses/{busId:guid}")]
        public async Task<ActionResult<ApiResponse<OperatorBusListItemDto>>> UpdateBus(Guid busId, [FromBody] UpdateBusRequestDto request)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<OperatorBusListItemDto>.FailureResponse("Not authorized"));

            var bus = await _context.Buses
                .FirstOrDefaultAsync(b => b.BusId == busId && b.OperatorId == operatorId.Value);

            if (bus == null)
                return NotFound(ApiResponse<OperatorBusListItemDto>.FailureResponse("Bus not found"));

            if (request.BusType.HasValue) bus.BusType = request.BusType.Value;
            if (request.BusCategory.HasValue) bus.BusCategory = request.BusCategory.Value;
            if (request.TotalSeats.HasValue) bus.TotalSeats = request.TotalSeats.Value;
            if (!string.IsNullOrEmpty(request.RegistrationNumber)) bus.RegistrationNumber = request.RegistrationNumber;
            if (request.Amenities != null) bus.AmenitiesJson = JsonSerializer.Serialize(request.Amenities);

            await _context.SaveChangesAsync();

            return await GetBusDetails(busId);
        }

        // DELETE: api/operator/buses/{busId}
        [HttpDelete("buses/{busId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> DeleteBus(Guid busId)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<MessageResponse>.FailureResponse("Not authorized"));

            var bus = await _context.Buses
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.Trips)
                .FirstOrDefaultAsync(b => b.BusId == busId && b.OperatorId == operatorId.Value);

            if (bus == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Bus not found"));

            // Check for active trips
            var hasActiveTrips = bus.Schedules
                .SelectMany(s => s.Trips)
                .Any(t => t.CurrentStatus == TripStatus.Scheduled || t.CurrentStatus == TripStatus.InTransit);

            if (hasActiveTrips)
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Cannot delete bus with active trips"));

            _context.Buses.Remove(bus);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = "Bus deleted" }));
        }

        // PATCH: api/operator/buses/{busId}/status
        [HttpPatch("buses/{busId:guid}/status")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> UpdateBusStatus(Guid busId, [FromBody] UpdateBusStatusRequestDto request)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<MessageResponse>.FailureResponse("Not authorized"));

            var bus = await _context.Buses
                .FirstOrDefaultAsync(b => b.BusId == busId && b.OperatorId == operatorId.Value);

            if (bus == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Bus not found"));

            bus.IsActive = request.IsActive;
            await _context.SaveChangesAsync();

            var status = request.IsActive ? "activated" : "deactivated";
            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = $"Bus {status}" }));
        }

        #endregion

        #region Seat Layout Management

        // POST: api/operator/buses/{busId}/seats
        [HttpPost("buses/{busId:guid}/seats")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> CreateSeatLayout(Guid busId, [FromBody] CreateSeatLayoutRequestDto request)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<MessageResponse>.FailureResponse("Not authorized"));

            var bus = await _context.Buses
                .Include(b => b.SeatLayouts)
                .FirstOrDefaultAsync(b => b.BusId == busId && b.OperatorId == operatorId.Value);

            if (bus == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Bus not found"));

            // Remove existing layouts
            _context.SeatLayouts.RemoveRange(bus.SeatLayouts);

            // Add new layouts
            foreach (var seat in request.Seats)
            {
                var seatLayout = new SeatLayout
                {
                    LayoutId = Guid.NewGuid(),
                    BusId = busId,
                    SeatNumber = seat.SeatNumber,
                    SeatType = seat.SeatType,
                    Deck = seat.Deck,
                    PositionX = seat.PositionX,
                    PositionY = seat.PositionY,
                    IsAvailable = seat.IsAvailable
                };
                _context.SeatLayouts.Add(seatLayout);
            }

            // Update total seats
            bus.TotalSeats = request.Seats.Count;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse
            {
                Success = true,
                Message = $"Seat layout created with {request.Seats.Count} seats"
            }));
        }

        // PUT: api/operator/buses/{busId}/seats/{seatId}
        [HttpPut("buses/{busId:guid}/seats/{seatId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> UpdateSeat(Guid busId, Guid seatId, [FromBody] UpdateSeatRequestDto request)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<MessageResponse>.FailureResponse("Not authorized"));

            var seat = await _context.SeatLayouts
                .Include(s => s.Bus)
                .FirstOrDefaultAsync(s => s.LayoutId == seatId && s.BusId == busId && s.Bus.OperatorId == operatorId.Value);

            if (seat == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Seat not found"));

            if (!string.IsNullOrEmpty(request.SeatNumber)) seat.SeatNumber = request.SeatNumber;
            if (request.SeatType.HasValue) seat.SeatType = request.SeatType.Value;
            if (request.Deck.HasValue) seat.Deck = request.Deck.Value;
            if (request.PositionX.HasValue) seat.PositionX = request.PositionX.Value;
            if (request.PositionY.HasValue) seat.PositionY = request.PositionY.Value;
            if (request.IsAvailable.HasValue) seat.IsAvailable = request.IsAvailable.Value;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = "Seat updated" }));
        }

        // DELETE: api/operator/buses/{busId}/seats/{seatId}
        [HttpDelete("buses/{busId:guid}/seats/{seatId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> DeleteSeat(Guid busId, Guid seatId)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<MessageResponse>.FailureResponse("Not authorized"));

            var seat = await _context.SeatLayouts
                .Include(s => s.Bus)
                .FirstOrDefaultAsync(s => s.LayoutId == seatId && s.BusId == busId && s.Bus.OperatorId == operatorId.Value);

            if (seat == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Seat not found"));

            _context.SeatLayouts.Remove(seat);

            // Update bus total seats
            var bus = await _context.Buses.FindAsync(busId);
            if (bus != null) bus.TotalSeats--;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = "Seat deleted" }));
        }

        #endregion

        #region Route Management

        // GET: api/operator/routes
        [HttpGet("routes")]
        public async Task<ActionResult<ApiResponse<List<OperatorRouteListItemDto>>>> GetRoutes()
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<List<OperatorRouteListItemDto>>.FailureResponse("Not authorized"));

            var busIds = await _context.Buses
                .Where(b => b.OperatorId == operatorId.Value)
                .Select(b => b.BusId)
                .ToListAsync();

            var routeIds = await _context.Schedules
                .Where(s => busIds.Contains(s.BusId))
                .Select(s => s.RouteId)
                .Distinct()
                .ToListAsync();

            var routes = await _context.Routes
                .Include(r => r.Stops)
                .Where(r => routeIds.Contains(r.RouteId))
                .Select(r => new OperatorRouteListItemDto
                {
                    RouteId = r.RouteId,
                    SourceCity = r.SourceCity,
                    DestinationCity = r.DestinationCity,
                    DistanceKm = r.DistanceKm,
                    EstimatedDurationHours = r.EstimatedDurationHours,
                    StopsCount = r.Stops.Count
                })
                .ToListAsync();

            return Ok(ApiResponse<List<OperatorRouteListItemDto>>.SuccessResponse(routes));
        }

        // POST: api/operator/routes
        [HttpPost("routes")]
        public async Task<ActionResult<ApiResponse<CreateRouteResponseDto>>> CreateRoute([FromBody] CreateRouteRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CreateRouteResponseDto>.FailureResponse("Invalid input"));

            var route = new Models.Route
            {
                RouteId = Guid.NewGuid(),
                SourceCity = request.SourceCity,
                DestinationCity = request.DestinationCity,
                DistanceKm = request.DistanceKm,
                EstimatedDurationHours = request.EstimatedDurationHours,
                CreatedAt = DateTime.UtcNow
            };

            _context.Routes.Add(route);

            // Add stops if provided
            if (request.Stops != null && request.Stops.Any())
            {
                foreach (var stopDto in request.Stops)
                {
                    var stop = new Stop
                    {
                        StopId = Guid.NewGuid(),
                        RouteId = route.RouteId,
                        StopName = stopDto.StopName,
                        StopOrder = stopDto.StopOrder,
                        ArrivalTimeOffset = stopDto.ArrivalTimeOffset,
                        DepartureTimeOffset = stopDto.DepartureTimeOffset
                    };
                    _context.Stops.Add(stop);
                }
            }

            await _context.SaveChangesAsync();

            var response = new CreateRouteResponseDto
            {
                RouteId = route.RouteId,
                SourceCity = route.SourceCity,
                DestinationCity = route.DestinationCity,
                Message = "Route created successfully"
            };

            return CreatedAtAction(nameof(GetRoutes), ApiResponse<CreateRouteResponseDto>.SuccessResponse(response));
        }

        // PUT: api/operator/routes/{routeId}
        [HttpPut("routes/{routeId:guid}")]
        public async Task<ActionResult<ApiResponse<OperatorRouteListItemDto>>> UpdateRoute(Guid routeId, [FromBody] UpdateRouteRequestDto request)
        {
            var route = await _context.Routes
                .Include(r => r.Stops)
                .FirstOrDefaultAsync(r => r.RouteId == routeId);

            if (route == null)
                return NotFound(ApiResponse<OperatorRouteListItemDto>.FailureResponse("Route not found"));

            if (!string.IsNullOrEmpty(request.SourceCity)) route.SourceCity = request.SourceCity;
            if (!string.IsNullOrEmpty(request.DestinationCity)) route.DestinationCity = request.DestinationCity;
            if (request.DistanceKm.HasValue) route.DistanceKm = request.DistanceKm.Value;
            if (request.EstimatedDurationHours.HasValue) route.EstimatedDurationHours = request.EstimatedDurationHours.Value;

            await _context.SaveChangesAsync();

            var response = new OperatorRouteListItemDto
            {
                RouteId = route.RouteId,
                SourceCity = route.SourceCity,
                DestinationCity = route.DestinationCity,
                DistanceKm = route.DistanceKm,
                EstimatedDurationHours = route.EstimatedDurationHours,
                StopsCount = route.Stops.Count
            };

            return Ok(ApiResponse<OperatorRouteListItemDto>.SuccessResponse(response));
        }

        // DELETE: api/operator/routes/{routeId}
        [HttpDelete("routes/{routeId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> DeleteRoute(Guid routeId)
        {
            var route = await _context.Routes
                .Include(r => r.Schedules)
                .FirstOrDefaultAsync(r => r.RouteId == routeId);

            if (route == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Route not found"));

            if (route.Schedules.Any())
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Cannot delete route with active schedules"));

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = "Route deleted" }));
        }

        #endregion

        #region Schedule Management

        // GET: api/operator/schedules
        [HttpGet("schedules")]
        public async Task<ActionResult<ApiResponse<List<OperatorScheduleListItemDto>>>> GetSchedules()
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<List<OperatorScheduleListItemDto>>.FailureResponse("Not authorized"));

            var busIds = await _context.Buses
                .Where(b => b.OperatorId == operatorId.Value)
                .Select(b => b.BusId)
                .ToListAsync();

            var schedules = await _context.Schedules
                .Include(s => s.Bus)
                .Include(s => s.Route)
                .Where(s => busIds.Contains(s.BusId))
                .Select(s => new OperatorScheduleListItemDto
                {
                    ScheduleId = s.ScheduleId,
                    BusNumber = s.Bus.BusNumber,
                    Route = $"{s.Route.SourceCity} - {s.Route.DestinationCity}",
                    DepartureTime = s.DepartureTime,
                    ArrivalTime = s.ArrivalTime,
                    BaseFare = s.BaseFare,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<List<OperatorScheduleListItemDto>>.SuccessResponse(schedules));
        }

        // POST: api/operator/schedules
        [HttpPost("schedules")]
        public async Task<ActionResult<ApiResponse<CreateScheduleResponseDto>>> CreateSchedule([FromBody] CreateScheduleRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CreateScheduleResponseDto>.FailureResponse("Invalid input"));

            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<CreateScheduleResponseDto>.FailureResponse("Not authorized"));

            // Validate bus belongs to operator
            var bus = await _context.Buses
                .FirstOrDefaultAsync(b => b.BusId == request.BusId && b.OperatorId == operatorId.Value);

            if (bus == null)
                return NotFound(ApiResponse<CreateScheduleResponseDto>.FailureResponse("Bus not found"));

            // Validate route
            var route = await _context.Routes.FindAsync(request.RouteId);
            if (route == null)
                return NotFound(ApiResponse<CreateScheduleResponseDto>.FailureResponse("Route not found"));

            var schedule = new Schedule
            {
                ScheduleId = Guid.NewGuid(),
                BusId = request.BusId,
                RouteId = request.RouteId,
                DepartureTime = request.DepartureTime,
                ArrivalTime = request.ArrivalTime,
                BaseFare = request.BaseFare,
                AvailableDatesJson = request.AvailableDates != null
                    ? JsonSerializer.Serialize(request.AvailableDates)
                    : null,
                IsActive = request.IsActive
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            var response = new CreateScheduleResponseDto
            {
                ScheduleId = schedule.ScheduleId,
                BusId = schedule.BusId,
                RouteId = schedule.RouteId,
                Message = "Schedule created successfully"
            };

            return CreatedAtAction(nameof(GetScheduleDetails), new { scheduleId = schedule.ScheduleId },
                ApiResponse<CreateScheduleResponseDto>.SuccessResponse(response));
        }

        // GET: api/operator/schedules/{scheduleId}
        [HttpGet("schedules/{scheduleId:guid}")]
        public async Task<ActionResult<ApiResponse<OperatorScheduleDetailsDto>>> GetScheduleDetails(Guid scheduleId)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<OperatorScheduleDetailsDto>.FailureResponse("Not authorized"));

            var schedule = await _context.Schedules
                .Include(s => s.Bus)
                .Include(s => s.Route)
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.Bus.OperatorId == operatorId.Value);

            if (schedule == null)
                return NotFound(ApiResponse<OperatorScheduleDetailsDto>.FailureResponse("Schedule not found"));

            var response = new OperatorScheduleDetailsDto
            {
                ScheduleId = schedule.ScheduleId,
                BusId = schedule.BusId,
                BusNumber = schedule.Bus.BusNumber,
                RouteId = schedule.RouteId,
                SourceCity = schedule.Route.SourceCity,
                DestinationCity = schedule.Route.DestinationCity,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                BaseFare = schedule.BaseFare,
                AvailableDates = ParseDates(schedule.AvailableDatesJson),
                IsActive = schedule.IsActive
            };

            return Ok(ApiResponse<OperatorScheduleDetailsDto>.SuccessResponse(response));
        }

        // PUT: api/operator/schedules/{scheduleId}
        [HttpPut("schedules/{scheduleId:guid}")]
        public async Task<ActionResult<ApiResponse<OperatorScheduleDetailsDto>>> UpdateSchedule(Guid scheduleId, [FromBody] UpdateScheduleRequestDto request)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<OperatorScheduleDetailsDto>.FailureResponse("Not authorized"));

            var schedule = await _context.Schedules
                .Include(s => s.Bus)
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.Bus.OperatorId == operatorId.Value);

            if (schedule == null)
                return NotFound(ApiResponse<OperatorScheduleDetailsDto>.FailureResponse("Schedule not found"));

            if (request.DepartureTime.HasValue) schedule.DepartureTime = request.DepartureTime.Value;
            if (request.ArrivalTime.HasValue) schedule.ArrivalTime = request.ArrivalTime.Value;
            if (request.BaseFare.HasValue) schedule.BaseFare = request.BaseFare.Value;
            if (request.AvailableDates != null)
                schedule.AvailableDatesJson = JsonSerializer.Serialize(request.AvailableDates);

            await _context.SaveChangesAsync();

            return await GetScheduleDetails(scheduleId);
        }

        // DELETE: api/operator/schedules/{scheduleId}
        [HttpDelete("schedules/{scheduleId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> DeleteSchedule(Guid scheduleId)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<MessageResponse>.FailureResponse("Not authorized"));

            var schedule = await _context.Schedules
                .Include(s => s.Bus)
                .Include(s => s.Trips)
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.Bus.OperatorId == operatorId.Value);

            if (schedule == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Schedule not found"));

            var hasActiveTrips = schedule.Trips.Any(t =>
                t.CurrentStatus == TripStatus.Scheduled || t.CurrentStatus == TripStatus.InTransit);

            if (hasActiveTrips)
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Cannot delete schedule with active trips"));

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = "Schedule deleted" }));
        }

        // PATCH: api/operator/schedules/{scheduleId}/status
        [HttpPatch("schedules/{scheduleId:guid}/status")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> UpdateScheduleStatus(Guid scheduleId, [FromBody] UpdateScheduleStatusRequestDto request)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<MessageResponse>.FailureResponse("Not authorized"));

            var schedule = await _context.Schedules
                .Include(s => s.Bus)
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.Bus.OperatorId == operatorId.Value);

            if (schedule == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Schedule not found"));

            schedule.IsActive = request.IsActive;
            await _context.SaveChangesAsync();

            var status = request.IsActive ? "activated" : "deactivated";
            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = $"Schedule {status}" }));
        }

        #endregion

        #region Trip Management

        // GET: api/operator/trips
        [HttpGet("trips")]
        public async Task<ActionResult<ApiResponse<List<OperatorTripListItemDto>>>> GetTrips([FromQuery] DateTime? date)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<List<OperatorTripListItemDto>>.FailureResponse("Not authorized"));

            var busIds = await _context.Buses
                .Where(b => b.OperatorId == operatorId.Value)
                .Select(b => b.BusId)
                .ToListAsync();

            var tripsQuery = _context.Trips
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Bus)
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Route)
                .Include(t => t.Bookings)
                .Where(t => busIds.Contains(t.Schedule.BusId));

            if (date.HasValue)
            {
                tripsQuery = tripsQuery.Where(t => t.TripDate.Date == date.Value.Date);
            }

            var tripsData = await tripsQuery
                .OrderByDescending(t => t.TripDate)
                .ToListAsync();

            var trips = tripsData.Select(t => new OperatorTripListItemDto
            {
                TripId = t.TripId,
                ScheduleId = t.ScheduleId,
                BusNumber = t.Schedule.Bus.BusNumber,
                Route = $"{t.Schedule.Route.SourceCity} - {t.Schedule.Route.DestinationCity}",
                TripDate = t.TripDate,
                DepartureDateTime = t.DepartureDateTime,
                ArrivalDateTime = t.ArrivalDateTime,
                CurrentStatus = t.CurrentStatus.ToString(),
                AvailableSeats = t.AvailableSeats,
                BookedSeats = t.Bookings.Where(b => b.BookingStatus == BookingStatus.Confirmed).Sum(b => b.TotalSeats)
            }).ToList();

            return Ok(ApiResponse<List<OperatorTripListItemDto>>.SuccessResponse(trips));
        }

        // GET: api/operator/trips/{tripId}
        [HttpGet("trips/{tripId:guid}")]
        public async Task<ActionResult<ApiResponse<OperatorTripDetailsDto>>> GetTripDetails(Guid tripId)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<OperatorTripDetailsDto>.FailureResponse("Not authorized"));

            var trip = await _context.Trips
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Bus)
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Route)
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.TripId == tripId && t.Schedule.Bus.OperatorId == operatorId.Value);

            if (trip == null)
                return NotFound(ApiResponse<OperatorTripDetailsDto>.FailureResponse("Trip not found"));

            var confirmedBookings = trip.Bookings.Where(b => b.BookingStatus == BookingStatus.Confirmed).ToList();

            var response = new OperatorTripDetailsDto
            {
                TripId = trip.TripId,
                ScheduleId = trip.ScheduleId,
                BusNumber = trip.Schedule.Bus.BusNumber,
                SourceCity = trip.Schedule.Route.SourceCity,
                DestinationCity = trip.Schedule.Route.DestinationCity,
                TripDate = trip.TripDate,
                DepartureDateTime = trip.DepartureDateTime,
                ArrivalDateTime = trip.ArrivalDateTime,
                CurrentStatus = trip.CurrentStatus.ToString(),
                TotalSeats = trip.Schedule.Bus.TotalSeats,
                AvailableSeats = trip.AvailableSeats,
                BookedSeats = confirmedBookings.Sum(b => b.TotalSeats),
                Revenue = confirmedBookings.Sum(b => b.TotalFare)
            };

            return Ok(ApiResponse<OperatorTripDetailsDto>.SuccessResponse(response));
        }

        // PATCH: api/operator/trips/{tripId}/status
        [HttpPatch("trips/{tripId:guid}/status")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> UpdateTripStatus(Guid tripId, [FromBody] UpdateTripStatusRequestDto request)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<MessageResponse>.FailureResponse("Not authorized"));

            var trip = await _context.Trips
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Bus)
                .FirstOrDefaultAsync(t => t.TripId == tripId && t.Schedule.Bus.OperatorId == operatorId.Value);

            if (trip == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Trip not found"));

            trip.CurrentStatus = request.CurrentStatus;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse
            {
                Success = true,
                Message = $"Trip status updated to {request.CurrentStatus}"
            }));
        }

        // POST: api/operator/trips/{tripId}/cancel
        [HttpPost("trips/{tripId:guid}/cancel")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> CancelTrip(Guid tripId, [FromBody] CancelTripRequestDto request)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<MessageResponse>.FailureResponse("Not authorized"));

            var trip = await _context.Trips
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Bus)
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.TripId == tripId && t.Schedule.Bus.OperatorId == operatorId.Value);

            if (trip == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Trip not found"));

            if (trip.CurrentStatus == TripStatus.Completed || trip.CurrentStatus == TripStatus.Cancelled)
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Cannot cancel completed or already cancelled trip"));

            trip.CurrentStatus = TripStatus.Cancelled;

            // Cancel all confirmed bookings
            foreach (var booking in trip.Bookings.Where(b => b.BookingStatus == BookingStatus.Confirmed))
            {
                booking.BookingStatus = BookingStatus.Cancelled;
            }

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse
            {
                Success = true,
                Message = $"Trip cancelled. {trip.Bookings.Count} bookings affected."
            }));
        }

        // GET: api/operator/trips/{tripId}/bookings
        [HttpGet("trips/{tripId:guid}/bookings")]
        public async Task<ActionResult<ApiResponse<List<TripBookingListItemDto>>>> GetTripBookings(Guid tripId)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<List<TripBookingListItemDto>>.FailureResponse("Not authorized"));

            var trip = await _context.Trips
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Bus)
                .FirstOrDefaultAsync(t => t.TripId == tripId && t.Schedule.Bus.OperatorId == operatorId.Value);

            if (trip == null)
                return NotFound(ApiResponse<List<TripBookingListItemDto>>.FailureResponse("Trip not found"));

            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.TripId == tripId)
                .Select(b => new TripBookingListItemDto
                {
                    BookingId = b.BookingId,
                    BookingReference = b.BookingReference,
                    PassengerName = $"{b.User.FirstName} {b.User.LastName}",
                    TotalSeats = b.TotalSeats,
                    TotalFare = b.TotalFare,
                    BookingStatus = b.BookingStatus.ToString(),
                    BookingDate = b.BookingDate
                })
                .ToListAsync();

            return Ok(ApiResponse<List<TripBookingListItemDto>>.SuccessResponse(bookings));
        }

        // GET: api/operator/trips/{tripId}/passengers
        [HttpGet("trips/{tripId:guid}/passengers")]
        public async Task<ActionResult<ApiResponse<List<TripPassengerDto>>>> GetTripPassengers(Guid tripId)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<List<TripPassengerDto>>.FailureResponse("Not authorized"));

            var trip = await _context.Trips
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Bus)
                .FirstOrDefaultAsync(t => t.TripId == tripId && t.Schedule.Bus.OperatorId == operatorId.Value);

            if (trip == null)
                return NotFound(ApiResponse<List<TripPassengerDto>>.FailureResponse("Trip not found"));

            var passengers = await _context.BookedSeats
                .Include(bs => bs.Booking)
                .Where(bs => bs.Booking.TripId == tripId && bs.Booking.BookingStatus == BookingStatus.Confirmed)
                .ToListAsync();

            var stopIds = passengers.SelectMany(p => new[] { p.BoardingPointId, p.DroppingPointId }).Distinct();
            var stops = await _context.Stops.Where(s => stopIds.Contains(s.StopId)).ToDictionaryAsync(s => s.StopId, s => s.StopName);

            var result = passengers.Select(p => new TripPassengerDto
            {
                SeatNumber = p.SeatNumber,
                PassengerName = p.PassengerName,
                PassengerAge = p.PassengerAge,
                PassengerGender = p.PassengerGender,
                BoardingPoint = stops.GetValueOrDefault(p.BoardingPointId, ""),
                DroppingPoint = stops.GetValueOrDefault(p.DroppingPointId, ""),
                BookingReference = p.Booking.BookingReference
            }).ToList();

            return Ok(ApiResponse<List<TripPassengerDto>>.SuccessResponse(result));
        }

        #endregion

        #region Revenue & Bookings

        // GET: api/operator/bookings
        [HttpGet("bookings")]
        public async Task<ActionResult<ApiResponse<List<OperatorBookingListItemDto>>>> GetOperatorBookings([FromQuery] PaginationQuery pagination)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<List<OperatorBookingListItemDto>>.FailureResponse("Not authorized"));

            var busIds = await _context.Buses
                .Where(b => b.OperatorId == operatorId.Value)
                .Select(b => b.BusId)
                .ToListAsync();

            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                        .ThenInclude(s => s.Route)
                .Include(b => b.Payment)
                .Where(b => busIds.Contains(b.Trip.Schedule.BusId))
                .OrderByDescending(b => b.BookingDate)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(b => new OperatorBookingListItemDto
                {
                    BookingId = b.BookingId,
                    BookingReference = b.BookingReference,
                    CustomerName = $"{b.User.FirstName} {b.User.LastName}",
                    Route = $"{b.Trip.Schedule.Route.SourceCity} - {b.Trip.Schedule.Route.DestinationCity}",
                    TravelDate = b.Trip.TripDate,
                    TotalSeats = b.TotalSeats,
                    TotalFare = b.TotalFare,
                    BookingStatus = b.BookingStatus.ToString(),
                    PaymentStatus = b.Payment != null ? b.Payment.PaymentStatus.ToString() : "Pending",
                    BookingDate = b.BookingDate
                })
                .ToListAsync();

            return Ok(ApiResponse<List<OperatorBookingListItemDto>>.SuccessResponse(bookings));
        }

        // GET: api/operator/bookings/{bookingId}
        [HttpGet("bookings/{bookingId:guid}")]
        public async Task<ActionResult<ApiResponse<OperatorBookingListItemDto>>> GetOperatorBookingDetails(Guid bookingId)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<OperatorBookingListItemDto>.FailureResponse("Not authorized"));

            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                        .ThenInclude(s => s.Bus)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                        .ThenInclude(s => s.Route)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.Trip.Schedule.Bus.OperatorId == operatorId.Value);

            if (booking == null)
                return NotFound(ApiResponse<OperatorBookingListItemDto>.FailureResponse("Booking not found"));

            var response = new OperatorBookingListItemDto
            {
                BookingId = booking.BookingId,
                BookingReference = booking.BookingReference,
                CustomerName = $"{booking.User.FirstName} {booking.User.LastName}",
                Route = $"{booking.Trip.Schedule.Route.SourceCity} - {booking.Trip.Schedule.Route.DestinationCity}",
                TravelDate = booking.Trip.TripDate,
                TotalSeats = booking.TotalSeats,
                TotalFare = booking.TotalFare,
                BookingStatus = booking.BookingStatus.ToString(),
                PaymentStatus = booking.Payment != null ? booking.Payment.PaymentStatus.ToString() : "Pending",
                BookingDate = booking.BookingDate
            };

            return Ok(ApiResponse<OperatorBookingListItemDto>.SuccessResponse(response));
        }

        // GET: api/operator/revenue
        [HttpGet("revenue")]
        public async Task<ActionResult<ApiResponse<RevenueStatisticsDto>>> GetRevenueStatistics([FromQuery] RevenueQueryDto query)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<RevenueStatisticsDto>.FailureResponse("Not authorized"));

            var startDate = query.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = query.EndDate ?? DateTime.UtcNow;

            var busIds = await _context.Buses
                .Where(b => b.OperatorId == operatorId.Value)
                .Select(b => b.BusId)
                .ToListAsync();

            var allBookings = await _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                .Include(b => b.Cancellation)
                .Where(b => busIds.Contains(b.Trip.Schedule.BusId)
                         && b.BookingDate >= startDate
                         && b.BookingDate <= endDate)
                .ToListAsync();

            var confirmedBookings = allBookings.Where(b => b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Completed);
            var cancelledBookings = allBookings.Where(b => b.BookingStatus == BookingStatus.Cancelled);

            var response = new RevenueStatisticsDto
            {
                TotalRevenue = confirmedBookings.Sum(b => b.TotalFare),
                TotalBookings = allBookings.Count,
                TotalPassengers = confirmedBookings.Sum(b => b.TotalSeats),
                CancelledBookings = cancelledBookings.Count(),
                RefundedAmount = cancelledBookings.Where(b => b.Cancellation != null).Sum(b => b.Cancellation!.RefundAmount),
                NetRevenue = confirmedBookings.Sum(b => b.TotalFare) - cancelledBookings.Where(b => b.Cancellation != null).Sum(b => b.Cancellation!.RefundAmount),
                Breakdown = new List<RevenueBreakdownDto>()
            };

            // Group by period
            var groupBy = query.GroupBy?.ToLower() ?? "day";
            var grouped = confirmedBookings.GroupBy(b => groupBy switch
            {
                "week" => b.BookingDate.Date.AddDays(-(int)b.BookingDate.DayOfWeek),
                "month" => new DateTime(b.BookingDate.Year, b.BookingDate.Month, 1),
                _ => b.BookingDate.Date
            });

            response.Breakdown = grouped
                .OrderBy(g => g.Key)
                .Select(g => new RevenueBreakdownDto
                {
                    Period = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(b => b.TotalFare),
                    Bookings = g.Count()
                })
                .ToList();

            return Ok(ApiResponse<RevenueStatisticsDto>.SuccessResponse(response));
        }

        // GET: api/operator/revenue/export
        [HttpGet("revenue/export")]
        public async Task<ActionResult> ExportRevenue([FromQuery] RevenueQueryDto query, [FromQuery] string format = "csv")
        {
            var result = await GetRevenueStatistics(query);
            if (result.Result is not OkObjectResult okResult)
                return BadRequest("Failed to generate report");

            var data = (okResult.Value as ApiResponse<RevenueStatisticsDto>)?.Data;
            if (data == null)
                return BadRequest("No data available");

            // Generate CSV
            var csv = "Period,Revenue,Bookings\n";
            foreach (var item in data.Breakdown)
            {
                csv += $"{item.Period},{item.Revenue},{item.Bookings}\n";
            }

            csv += $"\nTotal Revenue,{data.TotalRevenue}\n";
            csv += $"Total Bookings,{data.TotalBookings}\n";
            csv += $"Net Revenue,{data.NetRevenue}\n";

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"revenue_report_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        #endregion

        #region Reviews Management

        // GET: api/operator/reviews
        [HttpGet("reviews")]
        public async Task<ActionResult<ApiResponse<List<OperatorReviewListItemDto>>>> GetOperatorReviews([FromQuery] PaginationQuery pagination)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<List<OperatorReviewListItemDto>>.FailureResponse("Not authorized"));

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Trip)
                    .ThenInclude(t => t.Schedule)
                        .ThenInclude(s => s.Route)
                .Where(r => r.OperatorId == operatorId.Value)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            var result = reviews.Select(r => new OperatorReviewListItemDto
            {
                ReviewId = r.ReviewId,
                CustomerName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Anonymous",
                Route = $"{r.Trip.Schedule.Route.SourceCity} - {r.Trip.Schedule.Route.DestinationCity}",
                TravelDate = r.Trip.TripDate,
                Rating = r.Rating,
                Comment = r.Comment,
                Response = null,
                CreatedAt = r.CreatedAt
            }).ToList();

            return Ok(ApiResponse<List<OperatorReviewListItemDto>>.SuccessResponse(result));
        }

        // GET: api/operator/reviews/stats
        [HttpGet("reviews/stats")]
        public async Task<ActionResult<ApiResponse<ReviewStatisticsDto>>> GetReviewStatistics()
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<ReviewStatisticsDto>.FailureResponse("Not authorized"));

            var reviews = await _context.Reviews
                .Where(r => r.OperatorId == operatorId.Value)
                .ToListAsync();

            var response = new ReviewStatisticsDto
            {
                AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0,
                TotalReviews = reviews.Count,
                FiveStarCount = reviews.Count(r => r.Rating == 5),
                FourStarCount = reviews.Count(r => r.Rating == 4),
                ThreeStarCount = reviews.Count(r => r.Rating == 3),
                TwoStarCount = reviews.Count(r => r.Rating == 2),
                OneStarCount = reviews.Count(r => r.Rating == 1)
            };

            return Ok(ApiResponse<ReviewStatisticsDto>.SuccessResponse(response));
        }

        // POST: api/operator/reviews/{reviewId}/respond
        [HttpPost("reviews/{reviewId:guid}/respond")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> RespondToReview(Guid reviewId, [FromBody] RespondToReviewRequestDto request)
        {
            var operatorId = await GetCurrentOperatorId();
            if (operatorId == null)
                return Unauthorized(ApiResponse<MessageResponse>.FailureResponse("Not authorized"));

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.OperatorId == operatorId.Value);

            if (review == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Review not found"));

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse
            {
                Success = true,
                Message = "Response recorded successfully"
            }));
        }

        #endregion

        #region Helper Methods

        private async Task<Guid?> GetCurrentOperatorId()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return null;

            var operatorEntity = await _context.BusOperators
                .FirstOrDefaultAsync(o => o.UserId == userId);

            return operatorEntity?.OperatorId;
        }

        private Guid GetCurrentUserId()
        {
            // TODO: Implement proper JWT authentication
            return Guid.Empty;
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

        private static List<DateTime> ParseDates(string? datesJson)
        {
            if (string.IsNullOrEmpty(datesJson))
                return new List<DateTime>();

            try
            {
                return JsonSerializer.Deserialize<List<DateTime>>(datesJson) ?? new List<DateTime>();
            }
            catch
            {
                return new List<DateTime>();
            }
        }

        #endregion
    }
}
