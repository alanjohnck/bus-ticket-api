using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Auth;
using BusBookingSystem.API.DTOs.Common;
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
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        public AuthController(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<RegisterResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<RegisterResponseDto>.FailureResponse("Invalid input", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return Conflict(ApiResponse<RegisterResponseDto>.FailureResponse("Email already registered"));

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Role = UserRole.Passenger,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // TODO: Send verification email via SMTP

            var response = new RegisterResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Message = "Registration successful. Please check your email to verify your account."
            };

            return CreatedAtAction(nameof(Register), ApiResponse<RegisterResponseDto>.SuccessResponse(response, "Registration successful"));
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LoginResponseDto>.FailureResponse("Invalid input"));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(ApiResponse<LoginResponseDto>.FailureResponse("Invalid email or password"));

            // Generate simple token and store in cache
            var token = GenerateToken();
            await _cache.SetStringAsync($"token:{token}", user.UserId.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

            var response = new LoginResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful"));
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> Logout([FromHeader(Name = "X-User-Token")] string? token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                await _cache.RemoveAsync($"token:{token}");
            }

            var response = new MessageResponse
            {
                Success = true,
                Message = "Logout successful"
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // POST: api/auth/verify-email
        [HttpPost("verify-email")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> VerifyEmail([FromBody] VerifyEmailRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Invalid input"));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("User not found"));

            // TODO: Verify the verification code (stored in cache or database)
            // For now, we'll just mark the user as verified
            // In production, compare request.VerificationCode with stored code

            user.IsVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = new MessageResponse
            {
                Success = true,
                Message = "Email verified successfully"
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // POST: api/auth/resend-verification
        [HttpPost("resend-verification")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> ResendVerification([FromBody] ResendVerificationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Invalid input"));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("User not found"));

            if (user.IsVerified)
                return BadRequest(ApiResponse<MessageResponse>.FailureResponse("Email is already verified"));

            // TODO: Generate new verification code and send via SMTP

            var response = new MessageResponse
            {
                Success = true,
                Message = "Verification email sent successfully"
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // Helper methods
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

        private static string GenerateToken()
        {
            // Generate a random token
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}
