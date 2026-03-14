using ControleCerto.DTOs.Notification;
using ControleCerto.Enums;
using ControleCerto.Errors;

namespace ControleCerto.Services.Interfaces
{
    public interface INotificationService
    {
        public Task<Result<ICollection<InfoNotificationResponse>>> GetRecentNotificationsAsync(int userId, bool? isRead, int maxNotifications = 5);
        public Task<Result<ICollection<InfoNotificationResponse>>> GetAllNotificationsAsync(int userId, int page = 1, int pageSize = 50, bool includeExpired = false);
        public Task<Result<ICollection<InfoNotificationResponse>>> SendUserNotificationAsync(CreateNotificationRequest notification, int userId);
        public Task<Result<bool>> SendPublicNotificationAsync(CreatePublicNotificationRequest notification, int adminId);
        public Task<Result<bool>> MarkAsReadAsync(ICollection<long> notificationIds, int userId);
        public Task<Result<bool>> DeleteNotificationAsync(long notificationId, int userId);
    }
}
