using ControleCerto.Enums;

namespace ControleCerto.DTOs.Notification
{
    public class InfoNotificationResponse
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationTypeEnum Type { get; set; }
        public string? ActionPath { get; set; }
        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
    }
}
