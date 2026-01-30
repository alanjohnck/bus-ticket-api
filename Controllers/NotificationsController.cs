using BusBookingSystem.API.Data;
using BusBookingSystem.API.DTOs.Common;
using BusBookingSystem.API.DTOs.Notification;
using BusBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/notifications
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<NotificationListItemDto>>>> GetNotifications([FromQuery] PaginationQuery pagination)
        {
            var userId = GetCurrentUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(n => new NotificationListItemDto
                {
                    NotificationId = n.NotificationId,
                    NotificationType = n.NotificationType.ToString(),
                    Subject = n.Subject,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    SentAt = n.SentAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<NotificationListItemDto>>.SuccessResponse(notifications));
        }

        // GET: api/notifications/{notificationId}
        [HttpGet("{notificationId:guid}")]
        public async Task<ActionResult<ApiResponse<NotificationDetailsDto>>> GetNotificationDetails(Guid notificationId)
        {
            var userId = GetCurrentUserId();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
                return NotFound(ApiResponse<NotificationDetailsDto>.FailureResponse("Notification not found"));

            var response = new NotificationDetailsDto
            {
                NotificationId = notification.NotificationId,
                NotificationType = notification.NotificationType.ToString(),
                Subject = notification.Subject,
                Message = notification.Message,
                IsRead = notification.IsRead,
                SentAt = notification.SentAt,
                CreatedAt = notification.CreatedAt
            };

            return Ok(ApiResponse<NotificationDetailsDto>.SuccessResponse(response));
        }

        // PATCH: api/notifications/{notificationId}/read
        [HttpPatch("{notificationId:guid}/read")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> MarkAsRead(Guid notificationId)
        {
            var userId = GetCurrentUserId();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Notification not found"));

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            var response = new MessageResponse
            {
                Success = true,
                Message = "Notification marked as read"
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // PATCH: api/notifications/read-all
        [HttpPatch("read-all")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();

            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            var response = new MessageResponse
            {
                Success = true,
                Message = $"{unreadNotifications.Count} notifications marked as read"
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // DELETE: api/notifications/{notificationId}
        [HttpDelete("{notificationId:guid}")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> DeleteNotification(Guid notificationId)
        {
            var userId = GetCurrentUserId();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
                return NotFound(ApiResponse<MessageResponse>.FailureResponse("Notification not found"));

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            var response = new MessageResponse
            {
                Success = true,
                Message = "Notification deleted"
            };

            return Ok(ApiResponse<MessageResponse>.SuccessResponse(response));
        }

        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponse<UnreadCountDto>>> GetUnreadCount()
        {
            var userId = GetCurrentUserId();

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            var response = new UnreadCountDto
            {
                UnreadCount = unreadCount
            };

            return Ok(ApiResponse<UnreadCountDto>.SuccessResponse(response));
        }

        // Helper methods
        private Guid GetCurrentUserId()
        {
            // TODO: Implement proper JWT authentication
            return Guid.Empty;
        }
    }
}
