using ControleCerto.Enums;

namespace ControleCerto.Modules.Tickets.DTOs
{
    public class TicketResponse
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string Subject { get; set; }
        public TicketStatusEnum Status { get; set; }
        public TicketPriorityEnum Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}

