using ControleCerto.Decorators;
using ControleCerto.DTOs.Notification;
using ControleCerto.Errors;
using ControleCerto.Extensions;
using ControleCerto.Models.Entities;
using ControleCerto.Services;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ExtractTokenInfo]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("GetRecentNotifications")]
        public async Task<IActionResult> GetRecentNotifications([FromQuery] bool? isRead)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _notificationService.GetRecentNotificationsAsync(userId, isRead);

            return result.HandleReturnResult();
        }

        [HttpGet("GetAllNotifications")]
        public async Task<IActionResult> GetAllNotifications([FromQuery] bool? isRead)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _notificationService.GetAllNotificationsAsync(userId);

            return result.HandleReturnResult();
        }

        [HttpPost("SendUserNotification")]
        public async Task<IActionResult> SendUserNotification([FromBody] CreateNotificationRequest notification)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _notificationService.SendUserNotificationAsync(notification, userId);

            return result.HandleReturnResult();
        }

        [HttpPost("SendPublicNotification")]
        public async Task<IActionResult> SendPublicNotification([FromBody] CreatePublicNotificationRequest notification)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _notificationService.SendPublicNotificationAsync(notification, userId);

            return result.HandleReturnResult();
        }


        [HttpPatch("MarkAsRead")]
        public async Task<IActionResult> MarkAsRead([FromBody] ReadNotificationsRequest readNotifications)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _notificationService.MarkAsReadAsync(readNotifications.notificationIds, userId);

            return result.HandleReturnResult();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification([FromRoute] long id)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _notificationService.DeleteNotificationAsync(id, userId);

            return result.HandleReturnResult();
        }

    }
}
