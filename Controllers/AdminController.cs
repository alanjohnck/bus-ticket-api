using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Admin;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        public AdminController(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        #region Dashboard & Analytics

        // GET: api/admin/dashboard
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetDashboard()
        {
            if (!await IsAdminUser())
                return Unauthorized(ApiResponse<AdminDashboardDto>.FailureResponse("Admin access required"));

            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var todayBookings = await _context.Bookings
                .Where(b => b.BookingDate.Date == today && b.BookingStatus == BookingStatus.Confirmed)
                .ToListAsync();

            var monthlyBookings = await _context.Bookings
                .Where(b => b.BookingDate >= monthStart && b.BookingStatus == BookingStatus.Confirmed)
                .ToListAsync();

            var response = new AdminDashboardDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalOperators = await _context.BusOperators.CountAsync(),
                PendingOperatorApprovals = await _context.BusOperators.CountAsync(o => !o.IsApproved),
                TotalBuses = await _context.Buses.CountAsync(),
                TotalRoutes = await _context.Routes.CountAsync(),
                TodayTrips = await _context.Trips.CountAsync(t => t.TripDate.Date == today),
                TotalBookingsToday = todayBookings.Count,
                TodayRevenue = todayBookings.Sum(b => b.TotalFare),
                MonthlyRevenue = monthlyBookings.Sum(b => b.TotalFare),
                ActiveDisputes = 0 // TODO: Add disputes table
            };

            return Ok(ApiResponse<AdminDashboardDto>.SuccessResponse(response));
        }

        // GET: api/admin/analytics
        [HttpGet("analytics")]
        public async Task<ActionResult<ApiResponse<AnalyticsResponseDto>>> GetAnalytics([FromQuery] AnalyticsQueryDto query)
        {
            var startDate = query.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = query.EndDate ?? DateTime.UtcNow;
            var metric = query.Metric?.ToLower() ?? "bookings";

            var response = new AnalyticsResponseDto
            {
                Metric = metric,
                StartDate = startDate,
                EndDate = endDate,
                DataPoints = new List<AnalyticsDataPointDto>()
            };

            switch (metric)
            {
                case "bookings":
                    var bookings = await _context.Bookings
                        .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate)
                        .GroupBy(b => b.BookingDate.Date)
                        .Select(g => new AnalyticsDataPointDto { Date = g.Key, Value = g.Count() })
                        .ToListAsync();
                    response.DataPoints = bookings;
                    response.TotalValue = bookings.Sum(b => b.Value);
                    break;

                case "revenue":
                    var revenue = await _context.Bookings
                        .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate && b.BookingStatus == BookingStatus.Confirmed)
                        .GroupBy(b => b.BookingDate.Date)
                        .Select(g => new AnalyticsDataPointDto { Date = g.Key, Value = g.Sum(b => b.TotalFare) })
                        .ToListAsync();
                    response.DataPoints = revenue;
                    response.TotalValue = revenue.Sum(r => r.Value);
                    break;

                case "users":
                    var users = await _context.Users
                        .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                        .GroupBy(u => u.CreatedAt.Date)
                        .Select(g => new AnalyticsDataPointDto { Date = g.Key, Value = g.Count() })
                        .ToListAsync();
                    response.DataPoints = users;
                    response.TotalValue = users.Sum(u => u.Value);
                    break;

                case "trips":
                    var trips = await _context.Trips
                        .Where(t => t.TripDate >= startDate && t.TripDate <= endDate)
                        .GroupBy(t => t.TripDate.Date)
                        .Select(g => new AnalyticsDataPointDto { Date = g.Key, Value = g.Count() })
                        .ToListAsync();
                    response.DataPoints = trips;
                    response.TotalValue = trips.Sum(t => t.Value);
                    break;
            }

            // Calculate previous period for comparison
            var previousPeriodStart = startDate.AddDays(-(endDate - startDate).Days);
            var previousPeriodEnd = startDate;

            decimal previousValue = metric switch
            {
                "bookings" => await _context.Bookings.CountAsync(b => b.BookingDate >= previousPeriodStart && b.BookingDate < previousPeriodEnd),
                "revenue" => await _context.Bookings.Where(b => b.BookingDate >= previousPeriodStart && b.BookingDate < previousPeriodEnd && b.BookingStatus == BookingStatus.Confirmed).SumAsync(b => b.TotalFare),
                "users" => await _context.Users.CountAsync(u => u.CreatedAt >= previousPeriodStart && u.CreatedAt < previousPeriodEnd),
                "trips" => await _context.Trips.CountAsync(t => t.TripDate >= previousPeriodStart && t.TripDate < previousPeriodEnd),
                _ => 0
            };

            response.PreviousPeriodValue = previousValue;
            response.ChangePercentage = previousValue > 0 ? ((response.TotalValue - previousValue) / previousValue) * 100 : 0;

            return Ok(ApiResponse<AnalyticsResponseDto>.SuccessResponse(response));
        }

        // GET: api/admin/reports
        [HttpGet("reports")]
        public async Task<ActionResult<ApiResponse<ReportResponseDto>>> GetReports([FromQuery] ReportQueryDto query)
        {
            var startDate = query.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = query.EndDate ?? DateTime.UtcNow;
            var reportType = query.ReportType?.ToLower() ?? "bookings";

            object data = reportType switch
            {
                "bookings" => await GetBookingsReport(startDate, endDate),
                "revenue" => await GetRevenueReport(startDate, endDate),
                "operators" => await GetOperatorsReport(),
                "users" => await GetUsersReport(startDate, endDate),
                _ => new { }
            };

            var response = new ReportResponseDto
            {
                ReportType = reportType,
                GeneratedAt = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                Data = data
            };

            return Ok(ApiResponse<ReportResponseDto>.SuccessResponse(response));
        }

        // GET: api/admin/reports/export
        [HttpGet("reports/export")]
        public async Task<ActionResult> ExportReport([FromQuery] ReportQueryDto query, [FromQuery] string format = "csv")
        {
            var result = await GetReports(query);
            // TODO: Implement CSV/PDF export
            return Ok("Export functionality to be implemented");
        }

        #endregion

        #region User Management

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<PagedResponse<AdminUserListItemDto>>>> GetUsers([FromQuery] PaginationQuery pagination, [FromQuery] string? role)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var userRole))
            {
                query = query.Where(u => u.Role == userRole);
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(u => new AdminUserListItemDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FullName = $"{u.FirstName} {u.LastName}",
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role.ToString(),
                    IsVerified = u.IsVerified,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            var response = new PagedResponse<AdminUserListItemDto>
            {
                Items = users,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
            };

            return Ok(ApiResponse<PagedResponse<AdminUserListItemDto>>.SuccessResponse(response));
        }

        // GET: api/admin/users/{userId}
        [HttpGet("users/{userId:guid}")]
        public async Task<ActionResult<ApiResponse<AdminUserDetailsDto>>> GetUserDetails(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Bookings)
                .Include(u => u.Reviews)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(ApiResponse<AdminUserDetailsDto>.FailureResponse("User not found"));

            var response = new AdminUserDetailsDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Role = user.Role.ToString(),
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                TotalBookings = user.Bookings?.Count ?? 0,
                TotalReviews = user.Reviews?.Count ?? 0
            };

            return Ok(ApiResponse<AdminUserDetailsDto>.SuccessResponse(response));
        }

        // PATCH: api/admin/users/{userId}/status
        [HttpPatch("users/{userId:guid}/status")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusRequestDto request)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("User not found"));

            user.IsVerified = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var status = request.IsActive ? "activated" : "deactivated";
            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = $"User {status}" }));
        }

        // DELETE: api/admin/users/{userId}
        [HttpDelete("users/{userId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> DeleteUser(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Bookings)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("User not found"));

            var hasActiveBookings = user.Bookings?.Any(b => b.BookingStatus == BookingStatus.Confirmed) ?? false;
            if (hasActiveBookings)
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Cannot delete user with active bookings"));

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = "User deleted" }));
        }

        // GET: api/admin/users/statistics
        [HttpGet("users/statistics")]
        public async Task<ActionResult<ApiResponse<UserStatisticsDto>>> GetUserStatistics()
        {
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var response = new UserStatisticsDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.IsVerified),
                VerifiedUsers = await _context.Users.CountAsync(u => u.IsVerified),
                PassengerCount = await _context.Users.CountAsync(u => u.Role == UserRole.Passenger),
                OperatorCount = await _context.Users.CountAsync(u => u.Role == UserRole.Operator),
                AdminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin),
                NewUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= monthStart)
            };

            return Ok(ApiResponse<UserStatisticsDto>.SuccessResponse(response));
        }

        #endregion

        #region Operator Management

        // GET: api/admin/operators
        [HttpGet("operators")]
        public async Task<ActionResult<ApiResponse<List<AdminOperatorListItemDto>>>> GetOperators([FromQuery] PaginationQuery pagination)
        {
            var operators = await _context.BusOperators
                .Include(o => o.Buses)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(o => new AdminOperatorListItemDto
                {
                    OperatorId = o.OperatorId,
                    CompanyName = o.CompanyName,
                    ContactEmail = o.ContactEmail,
                    ContactPhone = o.ContactPhone,
                    City = o.City,
                    Rating = o.Rating,
                    IsApproved = o.IsApproved,
                    TotalBuses = o.Buses.Count,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<AdminOperatorListItemDto>>.SuccessResponse(operators));
        }

        // GET: api/admin/operators/{operatorId}
        [HttpGet("operators/{operatorId:guid}")]
        public async Task<ActionResult<ApiResponse<AdminOperatorDetailsDto>>> GetOperatorDetails(Guid operatorId)
        {
            var operatorEntity = await _context.BusOperators
                .Include(o => o.Buses)
                    .ThenInclude(b => b.Schedules)
                .FirstOrDefaultAsync(o => o.OperatorId == operatorId);

            if (operatorEntity == null)
                return NotFound(ApiResponse<AdminOperatorDetailsDto>.FailureResponse("Operator not found"));

            var busIds = operatorEntity.Buses.Select(b => b.BusId).ToList();
            var totalRevenue = await _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                .Where(b => busIds.Contains(b.Trip.Schedule.BusId) && b.BookingStatus == BookingStatus.Confirmed)
                .SumAsync(b => b.TotalFare);

            var routeIds = operatorEntity.Buses
                .SelectMany(b => b.Schedules)
                .Select(s => s.RouteId)
                .Distinct()
                .Count();

            var response = new AdminOperatorDetailsDto
            {
                OperatorId = operatorEntity.OperatorId,
                UserId = operatorEntity.UserId,
                CompanyName = operatorEntity.CompanyName,
                LicenseNumber = operatorEntity.LicenseNumber,
                ContactEmail = operatorEntity.ContactEmail,
                ContactPhone = operatorEntity.ContactPhone,
                Address = operatorEntity.Address,
                City = operatorEntity.City,
                State = operatorEntity.State,
                Rating = operatorEntity.Rating,
                IsApproved = operatorEntity.IsApproved,
                CreatedAt = operatorEntity.CreatedAt,
                TotalBuses = operatorEntity.Buses.Count,
                ActiveBuses = operatorEntity.Buses.Count(b => b.IsActive),
                TotalRoutes = routeIds,
                TotalRevenue = totalRevenue
            };

            return Ok(ApiResponse<AdminOperatorDetailsDto>.SuccessResponse(response));
        }

        // POST: api/admin/operators/approve
        [HttpPost("operators/approve")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> ApproveOperator([FromBody] ApproveOperatorRequestDto request)
        {
            var operatorEntity = await _context.BusOperators.FindAsync(request.OperatorId);

            if (operatorEntity == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Operator not found"));

            operatorEntity.IsApproved = true;
            await _context.SaveChangesAsync();

            // TODO: Send approval notification email

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = "Operator approved" }));
        }

        // PATCH: api/admin/operators/{operatorId}/status
        [HttpPatch("operators/{operatorId:guid}/status")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> UpdateOperatorStatus(Guid operatorId, [FromBody] UpdateOperatorStatusRequestDto request)
        {
            var operatorEntity = await _context.BusOperators.FindAsync(operatorId);

            if (operatorEntity == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Operator not found"));

            operatorEntity.IsApproved = request.IsApproved;
            await _context.SaveChangesAsync();

            var status = request.IsApproved ? "approved" : "suspended";
            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = $"Operator {status}" }));
        }

        // DELETE: api/admin/operators/{operatorId}
        [HttpDelete("operators/{operatorId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> DeleteOperator(Guid operatorId)
        {
            var operatorEntity = await _context.BusOperators
                .Include(o => o.Buses)
                .FirstOrDefaultAsync(o => o.OperatorId == operatorId);

            if (operatorEntity == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Operator not found"));

            if (operatorEntity.Buses.Any(b => b.IsActive))
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Cannot delete operator with active buses"));

            _context.BusOperators.Remove(operatorEntity);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = "Operator deleted" }));
        }

        // GET: api/admin/operators/pending
        [HttpGet("operators/pending")]
        public async Task<ActionResult<ApiResponse<List<PendingOperatorDto>>>> GetPendingOperators()
        {
            var pendingOperators = await _context.BusOperators
                .Where(o => !o.IsApproved)
                .OrderBy(o => o.CreatedAt)
                .Select(o => new PendingOperatorDto
                {
                    OperatorId = o.OperatorId,
                    CompanyName = o.CompanyName,
                    LicenseNumber = o.LicenseNumber,
                    ContactEmail = o.ContactEmail,
                    ContactPhone = o.ContactPhone,
                    City = o.City,
                    State = o.State,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PendingOperatorDto>>.SuccessResponse(pendingOperators));
        }

        #endregion

        #region Booking Management

        // GET: api/admin/bookings
        [HttpGet("bookings")]
        public async Task<ActionResult<ApiResponse<List<AdminBookingListItemDto>>>> GetAllBookings([FromQuery] PaginationQuery pagination, [FromQuery] string? status)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                        .ThenInclude(s => s.Bus)
                            .ThenInclude(bus => bus.Operator)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                        .ThenInclude(s => s.Route)
                .Include(b => b.Payment)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
            {
                query = query.Where(b => b.BookingStatus == bookingStatus);
            }

            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(b => new AdminBookingListItemDto
                {
                    BookingId = b.BookingId,
                    BookingReference = b.BookingReference,
                    CustomerName = $"{b.User.FirstName} {b.User.LastName}",
                    OperatorName = b.Trip.Schedule.Bus.Operator.CompanyName,
                    Route = $"{b.Trip.Schedule.Route.SourceCity} - {b.Trip.Schedule.Route.DestinationCity}",
                    TravelDate = b.Trip.TripDate,
                    TotalSeats = b.TotalSeats,
                    TotalFare = b.TotalFare,
                    BookingStatus = b.BookingStatus.ToString(),
                    PaymentStatus = b.Payment != null ? b.Payment.PaymentStatus.ToString() : "Pending",
                    BookingDate = b.BookingDate
                })
                .ToListAsync();

            return Ok(ApiResponse<List<AdminBookingListItemDto>>.SuccessResponse(bookings));
        }

        // GET: api/admin/bookings/statistics
        [HttpGet("bookings/statistics")]
        public async Task<ActionResult<ApiResponse<BookingStatisticsDto>>> GetBookingStatistics()
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var allBookings = await _context.Bookings.ToListAsync();

            var response = new BookingStatisticsDto
            {
                TotalBookings = allBookings.Count,
                ConfirmedBookings = allBookings.Count(b => b.BookingStatus == BookingStatus.Confirmed),
                CancelledBookings = allBookings.Count(b => b.BookingStatus == BookingStatus.Cancelled),
                CompletedBookings = allBookings.Count(b => b.BookingStatus == BookingStatus.Completed),
                TotalRevenue = allBookings.Where(b => b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Completed).Sum(b => b.TotalFare),
                AverageBookingValue = allBookings.Any() ? allBookings.Average(b => b.TotalFare) : 0,
                BookingsToday = allBookings.Count(b => b.BookingDate.Date == today),
                BookingsThisWeek = allBookings.Count(b => b.BookingDate >= weekStart),
                BookingsThisMonth = allBookings.Count(b => b.BookingDate >= monthStart)
            };

            return Ok(ApiResponse<BookingStatisticsDto>.SuccessResponse(response));
        }

        // PATCH: api/admin/bookings/{bookingId}
        [HttpPatch("bookings/{bookingId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> ModifyBooking(Guid bookingId, [FromBody] AdminModifyBookingRequestDto request)
        {
            var booking = await _context.Bookings
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Booking not found"));

            if (request.BookingStatus.HasValue)
            {
                var oldStatus = booking.BookingStatus;
                booking.BookingStatus = request.BookingStatus.Value;

                // Update available seats if status changed
                if (oldStatus == BookingStatus.Confirmed && request.BookingStatus.Value == BookingStatus.Cancelled)
                {
                    booking.Trip.AvailableSeats += booking.TotalSeats;
                }
                else if (oldStatus == BookingStatus.Cancelled && request.BookingStatus.Value == BookingStatus.Confirmed)
                {
                    booking.Trip.AvailableSeats -= booking.TotalSeats;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = "Booking updated" }));
        }

        #endregion

        #region Payment Management

        // GET: api/admin/payments
        [HttpGet("payments")]
        public async Task<ActionResult<ApiResponse<List<AdminPaymentListItemDto>>>> GetAllPayments([FromQuery] PaginationQuery pagination, [FromQuery] string? status)
        {
            var query = _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
            {
                query = query.Where(p => p.PaymentStatus == paymentStatus);
            }

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(p => new AdminPaymentListItemDto
                {
                    PaymentId = p.PaymentId,
                    BookingReference = p.Booking.BookingReference,
                    CustomerName = $"{p.Booking.User.FirstName} {p.Booking.User.LastName}",
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    PaymentStatus = p.PaymentStatus.ToString(),
                    TransactionId = p.TransactionId,
                    PaymentDate = p.PaymentDate
                })
                .ToListAsync();

            return Ok(ApiResponse<List<AdminPaymentListItemDto>>.SuccessResponse(payments));
        }

        // GET: api/admin/payments/failed
        [HttpGet("payments/failed")]
        public async Task<ActionResult<ApiResponse<List<FailedPaymentDto>>>> GetFailedPayments()
        {
            var failedPayments = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.User)
                .Where(p => p.PaymentStatus == PaymentStatus.Failed)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new FailedPaymentDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    BookingReference = p.Booking.BookingReference,
                    CustomerName = $"{p.Booking.User.FirstName} {p.Booking.User.LastName}",
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    TransactionId = p.TransactionId,
                    PaymentDate = p.PaymentDate
                })
                .ToListAsync();

            return Ok(ApiResponse<List<FailedPaymentDto>>.SuccessResponse(failedPayments));
        }

        // GET: api/admin/payments/reconcile
        [HttpGet("payments/reconcile")]
        public async Task<ActionResult<ApiResponse<PaymentReconciliationDto>>> GetPaymentReconciliation([FromQuery] DateTime? date)
        {
            var targetDate = date?.Date ?? DateTime.UtcNow.Date;

            var payments = await _context.Payments
                .Where(p => p.PaymentDate.Date == targetDate)
                .ToListAsync();

            var response = new PaymentReconciliationDto
            {
                Date = targetDate,
                TotalTransactions = payments.Count,
                TotalAmount = payments.Sum(p => p.Amount),
                CompletedCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Completed),
                CompletedAmount = payments.Where(p => p.PaymentStatus == PaymentStatus.Completed).Sum(p => p.Amount),
                PendingCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Pending),
                PendingAmount = payments.Where(p => p.PaymentStatus == PaymentStatus.Pending).Sum(p => p.Amount),
                FailedCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Failed),
                FailedAmount = payments.Where(p => p.PaymentStatus == PaymentStatus.Failed).Sum(p => p.Amount)
            };

            return Ok(ApiResponse<PaymentReconciliationDto>.SuccessResponse(response));
        }

        // POST: api/admin/refunds/{refundId}/approve
        [HttpPost("refunds/{refundId:guid}/approve")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> ApproveRefund(Guid refundId, [FromBody] ApproveRefundRequestDto request)
        {
            var cancellation = await _context.Cancellations.FindAsync(refundId);

            if (cancellation == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Refund not found"));

            cancellation.RefundStatus = RefundStatus.Processed;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = "Refund approved and processed" }));
        }

        // POST: api/admin/refunds/{refundId}/reject
        [HttpPost("refunds/{refundId:guid}/reject")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> RejectRefund(Guid refundId, [FromBody] RejectRefundRequestDto request)
        {
            var cancellation = await _context.Cancellations.FindAsync(refundId);

            if (cancellation == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Refund not found"));

            cancellation.RefundStatus = RefundStatus.Rejected;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = $"Refund rejected: {request.Reason}" }));
        }

        #endregion

        #region Content Management - Offers

        // GET: api/admin/offers
        [HttpGet("offers")]
        public async Task<ActionResult<ApiResponse<List<AdminOfferListItemDto>>>> GetAllOffers([FromQuery] PaginationQuery pagination)
        {
            var offers = await _context.Offers
                .OrderByDescending(o => o.ValidTo)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(o => new AdminOfferListItemDto
                {
                    OfferId = o.OfferId,
                    OfferCode = o.OfferCode,
                    Description = o.Description,
                    DiscountType = o.DiscountType.ToString(),
                    DiscountValue = o.DiscountValue,
                    MinBookingAmount = o.MinBookingAmount,
                    MaxDiscount = o.MaxDiscount,
                    ValidFrom = o.ValidFrom,
                    ValidTo = o.ValidTo,
                    UsageLimit = o.UsageLimit,
                    TimesUsed = o.TimesUsed,
                    IsActive = o.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<List<AdminOfferListItemDto>>.SuccessResponse(offers));
        }

        // POST: api/admin/offers
        [HttpPost("offers")]
        public async Task<ActionResult<ApiResponse<AdminOfferListItemDto>>> CreateOffer([FromBody] CreateOfferRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AdminOfferListItemDto>.FailureResponse("Invalid input"));

            if (await _context.Offers.AnyAsync(o => o.OfferCode == request.OfferCode))
                return Conflict(ApiResponse<AdminOfferListItemDto>.FailureResponse("Offer code already exists"));

            var offer = new Offer
            {
                OfferId = Guid.NewGuid(),
                OfferCode = request.OfferCode,
                Description = request.Description,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MinBookingAmount = request.MinBookingAmount,
                MaxDiscount = request.MaxDiscount,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                UsageLimit = request.UsageLimit,
                TimesUsed = 0,
                IsActive = request.IsActive
            };

            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();

            var response = new AdminOfferListItemDto
            {
                OfferId = offer.OfferId,
                OfferCode = offer.OfferCode,
                Description = offer.Description,
                DiscountType = offer.DiscountType.ToString(),
                DiscountValue = offer.DiscountValue,
                MinBookingAmount = offer.MinBookingAmount,
                MaxDiscount = offer.MaxDiscount,
                ValidFrom = offer.ValidFrom,
                ValidTo = offer.ValidTo,
                UsageLimit = offer.UsageLimit,
                TimesUsed = offer.TimesUsed,
                IsActive = offer.IsActive
            };

            return CreatedAtAction(nameof(GetAllOffers), ApiResponse<AdminOfferListItemDto>.SuccessResponse(response, "Offer created"));
        }

        // PUT: api/admin/offers/{offerId}
        [HttpPut("offers/{offerId:guid}")]
        public async Task<ActionResult<ApiResponse<AdminOfferListItemDto>>> UpdateOffer(Guid offerId, [FromBody] UpdateOfferRequestDto request)
        {
            var offer = await _context.Offers.FindAsync(offerId);

            if (offer == null)
                return NotFound(ApiResponse<AdminOfferListItemDto>.FailureResponse("Offer not found"));

            if (!string.IsNullOrEmpty(request.Description)) offer.Description = request.Description;
            if (request.DiscountType.HasValue) offer.DiscountType = request.DiscountType.Value;
            if (request.DiscountValue.HasValue) offer.DiscountValue = request.DiscountValue.Value;
            if (request.MinBookingAmount.HasValue) offer.MinBookingAmount = request.MinBookingAmount.Value;
            if (request.MaxDiscount.HasValue) offer.MaxDiscount = request.MaxDiscount.Value;
            if (request.ValidFrom.HasValue) offer.ValidFrom = request.ValidFrom.Value;
            if (request.ValidTo.HasValue) offer.ValidTo = request.ValidTo.Value;
            if (request.UsageLimit.HasValue) offer.UsageLimit = request.UsageLimit.Value;
            if (request.IsActive.HasValue) offer.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            var response = new AdminOfferListItemDto
            {
                OfferId = offer.OfferId,
                OfferCode = offer.OfferCode,
                Description = offer.Description,
                DiscountType = offer.DiscountType.ToString(),
                DiscountValue = offer.DiscountValue,
                MinBookingAmount = offer.MinBookingAmount,
                MaxDiscount = offer.MaxDiscount,
                ValidFrom = offer.ValidFrom,
                ValidTo = offer.ValidTo,
                UsageLimit = offer.UsageLimit,
                TimesUsed = offer.TimesUsed,
                IsActive = offer.IsActive
            };

            return Ok(ApiResponse<AdminOfferListItemDto>.SuccessResponse(response, "Offer updated"));
        }

        // DELETE: api/admin/offers/{offerId}
        [HttpDelete("offers/{offerId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> DeleteOffer(Guid offerId)
        {
            var offer = await _context.Offers.FindAsync(offerId);

            if (offer == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Offer not found"));

            _context.Offers.Remove(offer);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(new MessageResponse { Success = true, Message = "Offer deleted" }));
        }

        #endregion

        #region Dispute Management

        // GET: api/admin/disputes
        [HttpGet("disputes")]
        public async Task<ActionResult<ApiResponse<List<DisputeListItemDto>>>> GetDisputes()
        {
            // TODO: Implement disputes table and return data
            // For now, returning empty list as disputes table doesn't exist
            return Ok(ApiResponse<List<DisputeListItemDto>>.SuccessResponse(new List<DisputeListItemDto>()));
        }

        // GET: api/admin/disputes/{disputeId}
        [HttpGet("disputes/{disputeId:guid}")]
        public async Task<ActionResult<ApiResponse<DisputeDetailsDto>>> GetDisputeDetails(Guid disputeId)
        {
            // TODO: Implement disputes table
            return NotFound(ApiResponse<DisputeDetailsDto>.FailureResponse("Dispute not found"));
        }

        // PATCH: api/admin/disputes/{disputeId}
        [HttpPatch("disputes/{disputeId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> UpdateDisputeStatus(Guid disputeId, [FromBody] UpdateDisputeStatusRequestDto request)
        {
            // TODO: Implement disputes table
            return NotFound(ApiResponse<MessageResponse>.FailureResponse("Dispute not found"));
        }

        // POST: api/admin/disputes/{disputeId}/resolve
        [HttpPost("disputes/{disputeId:guid}/resolve")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> ResolveDispute(Guid disputeId, [FromBody] ResolveDisputeRequestDto request)
        {
            // TODO: Implement disputes table
            return NotFound(ApiResponse<MessageResponse>.FailureResponse("Dispute not found"));
        }

        #endregion

        #region Helper Methods

        private async Task<object> GetBookingsReport(DateTime startDate, DateTime endDate)
        {
            var bookings = await _context.Bookings
                .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate)
                .ToListAsync();

            return new
            {
                TotalBookings = bookings.Count,
                ConfirmedBookings = bookings.Count(b => b.BookingStatus == BookingStatus.Confirmed),
                CancelledBookings = bookings.Count(b => b.BookingStatus == BookingStatus.Cancelled),
                CompletedBookings = bookings.Count(b => b.BookingStatus == BookingStatus.Completed),
                TotalRevenue = bookings.Where(b => b.BookingStatus != BookingStatus.Cancelled).Sum(b => b.TotalFare),
                AverageBookingValue = bookings.Any() ? bookings.Average(b => b.TotalFare) : 0
            };
        }

        private async Task<object> GetRevenueReport(DateTime startDate, DateTime endDate)
        {
            var confirmedBookings = await _context.Bookings
                .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate && b.BookingStatus != BookingStatus.Cancelled)
                .ToListAsync();

            var cancellations = await _context.Cancellations
                .Where(c => c.CancellationDate >= startDate && c.CancellationDate <= endDate)
                .ToListAsync();

            return new
            {
                GrossRevenue = confirmedBookings.Sum(b => b.TotalFare),
                TotalRefunds = cancellations.Sum(c => c.RefundAmount),
                NetRevenue = confirmedBookings.Sum(b => b.TotalFare) - cancellations.Sum(c => c.RefundAmount),
                TotalTransactions = confirmedBookings.Count
            };
        }

        private async Task<object> GetOperatorsReport()
        {
            var operators = await _context.BusOperators
                .Include(o => o.Buses)
                .ToListAsync();

            return new
            {
                TotalOperators = operators.Count,
                ApprovedOperators = operators.Count(o => o.IsApproved),
                PendingApproval = operators.Count(o => !o.IsApproved),
                TotalBuses = operators.Sum(o => o.Buses.Count),
                AverageRating = operators.Any() ? operators.Average(o => o.Rating) : 0
            };
        }

        private async Task<object> GetUsersReport(DateTime startDate, DateTime endDate)
        {
            var users = await _context.Users
                .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                .ToListAsync();

            return new
            {
                NewUsers = users.Count,
                VerifiedUsers = users.Count(u => u.IsVerified),
                Passengers = users.Count(u => u.Role == UserRole.Passenger),
                Operators = users.Count(u => u.Role == UserRole.Operator),
                Admins = users.Count(u => u.Role == UserRole.Admin)
            };
        }

        private async Task<bool> IsAdminUser()
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
                return false;

            var user = await _context.Users.FindAsync(userId);
            return user?.Role == UserRole.Admin;
        }

        private async Task<Guid?> GetCurrentUserId()
        {
            var token = Request.Headers["X-User-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
                return null;

            var userIdString = await _cache.GetStringAsync($"token:{token}");
            if (string.IsNullOrEmpty(userIdString))
                return null;

            if (Guid.TryParse(userIdString, out var userId))
                return userId;

            return null;
        }

        #endregion
    }
}
