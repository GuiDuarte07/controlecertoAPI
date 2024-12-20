using ControleCerto.Enums;

namespace ControleCerto.Models.Entities
{
    public class Notification
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationTypeEnum Type { get; set; }
        public string? ActionPath { get; set; }
        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public Notification() { }

        public Notification(string title, string message, NotificationTypeEnum type, string? actionPath, DateTime expiresAt, int userId)
        {
            Title = title;
            Message = message;
            Type = type;
            ActionPath = actionPath;
            ExpiresAt = expiresAt;
            UserId = userId;
        }
    }
}
