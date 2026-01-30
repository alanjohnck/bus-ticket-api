using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.API.DTOs.User
{
    // GET/PUT /api/users/profile
    public class UserProfileDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileRequestDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }

    // PATCH /api/users/profile/password
    public class ChangePasswordRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    // User booking history item
    public class UserBookingHistoryDto
    {
        public Guid BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime TravelDate { get; set; }
        public int TotalSeats { get; set; }
        public decimal TotalFare { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }

    // User review item
    public class UserReviewDto
    {
        public Guid ReviewId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
 