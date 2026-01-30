using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Booking;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private static readonly Dictionary<string, (Guid TripId, List<string> Seats, DateTime ExpiresAt)> _seatHolds = new();

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CreateBookingResponseDto>>> CreateBooking([FromBody] CreateBookingRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CreateBookingResponseDto>.FailureResponse("Invalid input"));

            // Validate trip
            var trip = await _context.Trips
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Route)
                .Include(t => t.Bookings)
                    .ThenInclude(b => b.BookedSeats)
                .FirstOrDefaultAsync(t => t.TripId == request.TripId);

            if (trip == null)
                return NotFound(ApiResponse<CreateBookingResponseDto>.FailureResponse("Trip not found"));

            if (trip.CurrentStatus != TripStatus.Scheduled)
                return BadRequest(ApiResponse<CreateBookingResponseDto>.FailureResponse("Trip is not available for booking"));

            // Validate seats
            if (request.Seats.Count != request.Passengers.Count)
                return BadRequest(ApiResponse<CreateBookingResponseDto>.FailureResponse("Number of seats must match number of passengers"));

            // Check seat availability
            var bookedSeatNumbers = trip.Bookings
                .Where(b => b.BookingStatus == BookingStatus.Confirmed)
                .SelectMany(b => b.BookedSeats)
                .Select(bs => bs.SeatNumber)
                .ToHashSet();

            var unavailableSeats = request.Seats.Where(s => bookedSeatNumbers.Contains(s)).ToList();
            if (unavailableSeats.Any())
                return BadRequest(ApiResponse<CreateBookingResponseDto>.FailureResponse($"Seats already booked: {string.Join(", ", unavailableSeats)}"));

            // Validate boarding and dropping points
            var boardingPoint = await _context.Stops.FindAsync(request.BoardingPoint);
            var droppingPoint = await _context.Stops.FindAsync(request.DroppingPoint);

            if (boardingPoint == null || droppingPoint == null)
                return BadRequest(ApiResponse<CreateBookingResponseDto>.FailureResponse("Invalid boarding or dropping point"));

            // Calculate fare
            var baseFare = trip.Schedule.BaseFare * request.Seats.Count;
            var taxAmount = baseFare * 0.05m;
            var serviceCharge = 25m;
            var totalFare = baseFare + taxAmount + serviceCharge;
            var discountAmount = 0m;

            // Apply offer if provided
            if (!string.IsNullOrEmpty(request.OfferCode))
            {
                var offer = await _context.Offers
                    .FirstOrDefaultAsync(o => o.OfferCode == request.OfferCode
                                           && o.IsActive
                                           && o.ValidFrom <= DateTime.UtcNow
                                           && o.ValidTo >= DateTime.UtcNow
                                           && o.TimesUsed < o.UsageLimit
                                           && totalFare >= o.MinBookingAmount);

                if (offer != null)
                {
                    if (offer.DiscountType == DiscountType.Percentage)
                    {
                        discountAmount = totalFare * (offer.DiscountValue / 100);
                        if (discountAmount > offer.MaxDiscount)
                            discountAmount = offer.MaxDiscount;
                    }
                    else
                    {
                        discountAmount = offer.DiscountValue;
                    }

                    offer.TimesUsed++;
                }
            }

            var finalAmount = totalFare - discountAmount;
            var userId = GetCurrentUserId();

            // Create booking
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                UserId = userId,
                TripId = request.TripId,
                BookingReference = GenerateBookingReference(),
                TotalSeats = request.Seats.Count,
                TotalFare = finalAmount,
                BookingStatus = BookingStatus.Confirmed,
                BookingDate = DateTime.UtcNow,
                PassengerDetailsJson = System.Text.Json.JsonSerializer.Serialize(request.Passengers)
            };

            _context.Bookings.Add(booking);

            // Create booked seats
            for (int i = 0; i < request.Passengers.Count; i++)
            {
                var passenger = request.Passengers[i];
                var bookedSeat = new BookedSeat
                {
                    BookedSeatId = Guid.NewGuid(),
                    BookingId = booking.BookingId,
                    SeatNumber = passenger.SeatNumber,
                    PassengerName = passenger.Name,
                    PassengerAge = passenger.Age,
                    PassengerGender = passenger.Gender,
                    BoardingPointId = request.BoardingPoint,
                    DroppingPointId = request.DroppingPoint
                };
                _context.BookedSeats.Add(bookedSeat);
            }

            // Update available seats
            trip.AvailableSeats -= request.Seats.Count;

            await _context.SaveChangesAsync();

            var response = new CreateBookingResponseDto
            {
                BookingId = booking.BookingId,
                BookingReference = booking.BookingReference,
                TotalFare = totalFare,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                BookingStatus = booking.BookingStatus.ToString(),
                BookingDate = booking.BookingDate,
                Message = "Booking created successfully. Please proceed with payment."
            };

            return CreatedAtAction(nameof(GetBookingDetails), new { bookingId = booking.BookingId },
                ApiResponse<CreateBookingResponseDto>.SuccessResponse(response, "Booking created successfully"));
        }

        // POST: api/bookings/hold-seats
        [HttpPost("hold-seats")]
        public async Task<ActionResult<ApiResponse<HoldSeatsResponseDto>>> HoldSeats([FromBody] HoldSeatsRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<HoldSeatsResponseDto>.FailureResponse("Invalid input"));

            var trip = await _context.Trips
                .Include(t => t.Bookings)
                    .ThenInclude(b => b.BookedSeats)
                .FirstOrDefaultAsync(t => t.TripId == request.TripId);

            if (trip == null)
                return NotFound(ApiResponse<HoldSeatsResponseDto>.FailureResponse("Trip not found"));

            // Check seat availability
            var bookedSeatNumbers = trip.Bookings
                .Where(b => b.BookingStatus == BookingStatus.Confirmed)
                .SelectMany(b => b.BookedSeats)
                .Select(bs => bs.SeatNumber)
                .ToHashSet();

            // Also check held seats
            var heldSeats = _seatHolds
                .Where(h => h.Value.TripId == request.TripId && h.Value.ExpiresAt > DateTime.UtcNow)
                .SelectMany(h => h.Value.Seats)
                .ToHashSet();

            var unavailableSeats = request.Seats.Where(s => bookedSeatNumbers.Contains(s) || heldSeats.Contains(s)).ToList();
            if (unavailableSeats.Any())
                return BadRequest(ApiResponse<HoldSeatsResponseDto>.FailureResponse($"Seats not available: {string.Join(", ", unavailableSeats)}"));

            var holdId = Guid.NewGuid().ToString("N")[..12].ToUpper();
            var expiresAt = DateTime.UtcNow.AddMinutes(10);

            _seatHolds[holdId] = (request.TripId, request.Seats, expiresAt);

            var response = new HoldSeatsResponseDto
            {
                HoldId = holdId,
                TripId = request.TripId,
                HeldSeats = request.Seats,
                ExpiresAt = expiresAt
            };

            return Ok(ApiResponse<HoldSeatsResponseDto>.SuccessResponse(response, "Seats held successfully for 10 minutes"));
        }

        // DELETE: api/bookings/release-seats
        [HttpDelete("release-seats")]
        public ActionResult<ApiResponse<MessageResponse>> ReleaseSeats([FromBody] ReleaseSeatsRequestDto request)
        {
            if (!_seatHolds.ContainsKey(request.HoldId))
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Hold not found"));

            _seatHolds.Remove(request.HoldId);

            var response = new MessageResponse
            {
                Success = true,
                Message = "Seats released successfully"
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // GET: api/bookings
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<BookingListItemDto>>>> GetAllBookings([FromQuery] PaginationQuery pagination)
        {
            var userId = GetCurrentUserId();

            var bookings = await _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                        .ThenInclude(s => s.Route)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(b => new BookingListItemDto
                {
                    BookingId = b.BookingId,
                    BookingReference = b.BookingReference,
                    Source = b.Trip.Schedule.Route.SourceCity,
                    Destination = b.Trip.Schedule.Route.DestinationCity,
                    TravelDate = b.Trip.TripDate,
                    TotalSeats = b.TotalSeats,
                    TotalFare = b.TotalFare,
                    BookingStatus = b.BookingStatus.ToString(),
                    BookingDate = b.BookingDate
                })
                .ToListAsync();

            return Ok(ApiResponse<List<BookingListItemDto>>.SuccessResponse(bookings));
        }

        // GET: api/bookings/{bookingId}
        [HttpGet("{bookingId:guid}")]
        public async Task<ActionResult<ApiResponse<BookingDetailsDto>>> GetBookingDetails(Guid bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                        .ThenInclude(s => s.Route)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                        .ThenInclude(s => s.Bus)
                            .ThenInclude(bus => bus.Operator)
                .Include(b => b.BookedSeats)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound(ApiResponse<BookingDetailsDto>.FailureResponse("Booking not found"));

            // Get stop names
            var boardingPointIds = booking.BookedSeats.Select(bs => bs.BoardingPointId).Distinct();
            var droppingPointIds = booking.BookedSeats.Select(bs => bs.DroppingPointId).Distinct();
            var stopIds = boardingPointIds.Concat(droppingPointIds).Distinct();
            var stops = await _context.Stops.Where(s => stopIds.Contains(s.StopId)).ToDictionaryAsync(s => s.StopId, s => s.StopName);

            var response = new BookingDetailsDto
            {
                BookingId = booking.BookingId,
                BookingReference = booking.BookingReference,
                TotalSeats = booking.TotalSeats,
                TotalFare = booking.TotalFare,
                BookingStatus = booking.BookingStatus.ToString(),
                BookingDate = booking.BookingDate,
                Trip = new TripSummaryDto
                {
                    TripId = booking.TripId,
                    Source = booking.Trip.Schedule.Route.SourceCity,
                    Destination = booking.Trip.Schedule.Route.DestinationCity,
                    DepartureTime = booking.Trip.DepartureDateTime,
                    ArrivalTime = booking.Trip.ArrivalDateTime,
                    BusNumber = booking.Trip.Schedule.Bus.BusNumber,
                    OperatorName = booking.Trip.Schedule.Bus.Operator.CompanyName
                },
                BookedSeats = booking.BookedSeats.Select(bs => new BookedSeatDto
                {
                    SeatNumber = bs.SeatNumber,
                    PassengerName = bs.PassengerName,
                    PassengerAge = bs.PassengerAge,
                    PassengerGender = bs.PassengerGender,
                    BoardingPoint = stops.GetValueOrDefault(bs.BoardingPointId, ""),
                    DroppingPoint = stops.GetValueOrDefault(bs.DroppingPointId, "")
                }).ToList(),
                Payment = booking.Payment != null ? new PaymentSummaryDto
                {
                    PaymentId = booking.Payment.PaymentId,
                    Amount = booking.Payment.Amount,
                    PaymentMethod = booking.Payment.PaymentMethod.ToString(),
                    PaymentStatus = booking.Payment.PaymentStatus.ToString(),
                    PaymentDate = booking.Payment.PaymentDate
                } : null
            };

            return Ok(ApiResponse<BookingDetailsDto>.SuccessResponse(response));
        }

        // GET: api/bookings/{bookingId}/ticket
        [HttpGet("{bookingId:guid}/ticket")]
        public async Task<ActionResult> GetETicket(Guid bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                        .ThenInclude(s => s.Route)
                .Include(b => b.BookedSeats)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound("Booking not found");

            if (booking.BookingStatus != BookingStatus.Confirmed)
                return BadRequest("Ticket not available for cancelled bookings");

            // TODO: Generate PDF ticket
            // For now, return a simple text representation
            var ticketContent = $@"
E-TICKET
========
Booking Reference: {booking.BookingReference}
Travel Date: {booking.Trip.TripDate:dd-MMM-yyyy}
Route: {booking.Trip.Schedule.Route.SourceCity} to {booking.Trip.Schedule.Route.DestinationCity}
Departure: {booking.Trip.DepartureDateTime:HH:mm}
Arrival: {booking.Trip.ArrivalDateTime:HH:mm}
Seats: {string.Join(", ", booking.BookedSeats.Select(s => s.SeatNumber))}
Total Fare: Rs. {booking.TotalFare:F2}
";

            return Content(ticketContent, "text/plain");
        }

        // PATCH: api/bookings/{bookingId}
        [HttpPatch("{bookingId:guid}")]
        public async Task<ActionResult<ApiResponse<BookingDetailsDto>>> ModifyBooking(Guid bookingId, [FromBody] ModifyBookingRequestDto request)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookedSeats)
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound(ApiResponse<BookingDetailsDto>.FailureResponse("Booking not found"));

            if (booking.BookingStatus != BookingStatus.Confirmed)
                return BadRequest(ApiResponse<BookingDetailsDto>.FailureResponse("Cannot modify cancelled/completed booking"));

            // Check if trip hasn't departed
            if (booking.Trip.DepartureDateTime <= DateTime.UtcNow)
                return BadRequest(ApiResponse<BookingDetailsDto>.FailureResponse("Cannot modify booking after departure"));

            // Update boarding/dropping points
            if (request.BoardingPoint.HasValue)
            {
                var boardingPoint = await _context.Stops.FindAsync(request.BoardingPoint.Value);
                if (boardingPoint == null)
                    return BadRequest(ApiResponse<BookingDetailsDto>.FailureResponse("Invalid boarding point"));

                foreach (var seat in booking.BookedSeats)
                {
                    seat.BoardingPointId = request.BoardingPoint.Value;
                }
            }

            if (request.DroppingPoint.HasValue)
            {
                var droppingPoint = await _context.Stops.FindAsync(request.DroppingPoint.Value);
                if (droppingPoint == null)
                    return BadRequest(ApiResponse<BookingDetailsDto>.FailureResponse("Invalid dropping point"));

                foreach (var seat in booking.BookedSeats)
                {
                    seat.DroppingPointId = request.DroppingPoint.Value;
                }
            }

            await _context.SaveChangesAsync();

            return await GetBookingDetails(bookingId);
        }

        // DELETE: api/bookings/{bookingId}
        [HttpDelete("{bookingId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> CancelBooking(Guid bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Booking not found"));

            if (booking.BookingStatus != BookingStatus.Confirmed)
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Booking is already cancelled"));

            // Check if trip hasn't departed
            if (booking.Trip.DepartureDateTime <= DateTime.UtcNow)
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Cannot cancel booking after departure"));

            booking.BookingStatus = BookingStatus.Cancelled;
            booking.Trip.AvailableSeats += booking.TotalSeats;

            await _context.SaveChangesAsync();

            var response = new MessageResponse
            {
                Success = true,
                Message = "Booking cancelled successfully. Please check cancellation policy for refund."
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // GET: api/bookings/reference/{ref}
        [HttpGet("reference/{referenceNumber}")]
        public async Task<ActionResult<ApiResponse<BookingDetailsDto>>> GetBookingByReference(string referenceNumber)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingReference == referenceNumber);

            if (booking == null)
                return NotFound(ApiResponse<BookingDetailsDto>.FailureResponse("Booking not found"));

            return await GetBookingDetails(booking.BookingId);
        }

        // POST: api/bookings/{bookingId}/resend
        [HttpPost("{bookingId:guid}/resend")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> ResendConfirmation(Guid bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Booking not found"));

            // TODO: Send confirmation email via SMTP

            var response = new MessageResponse
            {
                Success = true,
                Message = $"Booking confirmation sent to {booking.User.Email}"
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // Helper methods
        private Guid GetCurrentUserId()
        {
            // TODO: Implement proper JWT authentication and extract user ID from claims
            return Guid.Empty;
        }

        private static string GenerateBookingReference()
        {
            return $"BK{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        }
    }
}
