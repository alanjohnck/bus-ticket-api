using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.DTOs.User;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        public UsersController(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/users/profile
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile([FromHeader(Name = "X-User-Token")] string? token)
        {
            var userId = await GetUserIdFromToken(token);

            if (userId == null)
                return Unauthorized(ApiResponse<UserProfileDto>.FailureResponse("Invalid or missing token. Please login first."));

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(ApiResponse<UserProfileDto>.FailureResponse("User not found"));

            var profile = new UserProfileDto
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
                CreatedAt = user.CreatedAt
            };

            return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile));
        }

        // GET: api/users/{userId}
        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetUserById(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(ApiResponse<UserProfileDto>.FailureResponse("User not found"));

            var profile = new UserProfileDto
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
                CreatedAt = user.CreatedAt
            };

            return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile));
        }

        // PUT: api/users/profile
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile(
            [FromHeader(Name = "X-User-Token")] string? token,
            [FromBody] UpdateProfileRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<UserProfileDto>.FailureResponse("Invalid input"));

            var userId = await GetUserIdFromToken(token);

            if (userId == null)
                return Unauthorized(ApiResponse<UserProfileDto>.FailureResponse("Invalid or missing token. Please login first."));

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(ApiResponse<UserProfileDto>.FailureResponse("User not found"));

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.DateOfBirth = request.DateOfBirth;
            user.Gender = request.Gender;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var profile = new UserProfileDto
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
                CreatedAt = user.CreatedAt
            };

            return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile, "Profile updated successfully"));
        }

        // PATCH: api/users/profile/password
        [HttpPatch("profile/password")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> ChangePassword(
            [FromHeader(Name = "X-User-Token")] string? token,
            [FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Invalid input"));

            var userId = await GetUserIdFromToken(token);

            if (userId == null)
                return Unauthorized(ApiResponse<MessageResponse>.FailureResponse("Invalid or missing token. Please login first."));

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("User not found"));

            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Current password is incorrect"));

            user.PasswordHash = HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new MessageResponse
            {
                Success = true,
                Message = "Password changed successfully"
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // DELETE: api/users/profile
        [HttpDelete("profile")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> DeleteAccount([FromHeader(Name = "X-User-Token")] string? token)
        {
            var userId = await GetUserIdFromToken(token);

            if (userId == null)
                return Unauthorized(ApiResponse<MessageResponse>.FailureResponse("Invalid or missing token. Please login first."));

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("User not found"));

            // Check for active bookings
            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => b.UserId == userId && b.BookingStatus == BookingStatus.Confirmed);

            if (hasActiveBookings)
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Cannot delete account with active bookings"));

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var response = new MessageResponse
            {
                Success = true,
                Message = "Account deleted successfully"
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // GET: api/users/profile/bookings
        [HttpGet("profile/bookings")]
        public async Task<ActionResult<ApiResponse<List<UserBookingHistoryDto>>>> GetUserBookings(
            [FromHeader(Name = "X-User-Token")] string? token,
            [FromQuery] PaginationQuery pagination)
        {
            var userId = await GetUserIdFromToken(token);

            if (userId == null)
                return Unauthorized(ApiResponse<List<UserBookingHistoryDto>>.FailureResponse("Invalid or missing token. Please login first."));

            var bookings = await _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Schedule)
                        .ThenInclude(s => s.Route)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(b => new UserBookingHistoryDto
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

            return Ok(ApiResponse<List<UserBookingHistoryDto>>.SuccessResponse(bookings));
        }

        // GET: api/users/profile/reviews
        [HttpGet("profile/reviews")]
        public async Task<ActionResult<ApiResponse<List<UserReviewDto>>>> GetUserReviews(
            [FromHeader(Name = "X-User-Token")] string? token,
            [FromQuery] PaginationQuery pagination)
        {
            var userId = await GetUserIdFromToken(token);

            if (userId == null)
                return Unauthorized(ApiResponse<List<UserReviewDto>>.FailureResponse("Invalid or missing token. Please login first."));

            var reviews = await _context.Reviews
                .Include(r => r.Operator)
                .Include(r => r.Trip)
                    .ThenInclude(t => t.Schedule)
                        .ThenInclude(s => s.Route)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            var result = reviews.Select(r => new UserReviewDto
            {
                ReviewId = r.ReviewId,
                OperatorName = r.Operator?.CompanyName ?? "Unknown",
                Route = $"{r.Trip.Schedule.Route.SourceCity} - {r.Trip.Schedule.Route.DestinationCity}",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList();

            return Ok(ApiResponse<List<UserReviewDto>>.SuccessResponse(result));
        }

        // Helper methods
        private async Task<Guid?> GetUserIdFromToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var userIdString = await _cache.GetStringAsync($"token:{token}");
            if (string.IsNullOrEmpty(userIdString))
                return null;

            if (Guid.TryParse(userIdString, out var userId))
                return userId;

            return null;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
