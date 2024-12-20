using ControleCerto.DTOs.Notification;
using ControleCerto.Enums;
using ControleCerto.Errors;

namespace ControleCerto.Services.Interfaces
{
    public interface INotificationService
    {
        public Task<Result<ICollection<InfoNotificationResponse>>> GetRecentNotificationsAsync(int userId, bool? isRead);
        public Task<Result<ICollection<InfoNotificationResponse>>> GetAllNotificationsAsync(int userId);
        public Task<Result<InfoNotificationResponse>> SendUserNotificationAsync(CreateNotificationRequest notification);
        public Task<Result<bool>> SendPublicNotificationAsync(CreatePublicNotificationRequest notification, int? adminId);
        public Task<Result<bool>> MarkAsReadAsync(ICollection<long> notificationIds, int userId);
        public Task<Result<bool>> DeleteNotificationAsync(long notificationId, int userId);
    }
}
