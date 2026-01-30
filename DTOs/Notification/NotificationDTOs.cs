using System.ComponentModel.DataAnnotations;
using BusBookingSystem.API.Models;

namespace BusBookingSystem.API.DTOs.Notification
{
    // GET /api/notifications
    public class NotificationListItemDto
    {
        public Guid NotificationId { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }

    // GET /api/notifications/:notificationId
    public class NotificationDetailsDto
    {
        public Guid NotificationId { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // GET /api/notifications/unread-count
    public class UnreadCountDto
    {
        public int UnreadCount { get; set; }
    }

    // Internal - for creating notifications
    public class CreateNotificationDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;
    }
}
