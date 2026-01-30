using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBookingSystem.API.Models
{
    public class Notification
    {
        [Key]
        public Guid NotificationId { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; }

        public NotificationType NotificationType { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}