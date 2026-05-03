using ControleCerto.Enums;

namespace ControleCerto.Models.Entities
{
    public class Ticket
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string Subject { get; set; }
        public TicketStatusEnum Status { get; set; } = TicketStatusEnum.OPEN;
        public TicketPriorityEnum Priority { get; set; } = TicketPriorityEnum.NORMAL;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
        public ICollection<TicketMessage> Messages { get; set; }
        public ICollection<TicketAttachment> Attachments { get; set; }
    }
}

