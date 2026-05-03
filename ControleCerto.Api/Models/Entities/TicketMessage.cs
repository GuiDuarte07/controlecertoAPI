using ControleCerto.Enums;

namespace ControleCerto.Models.Entities
{
    public class TicketMessage
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public Ticket? Ticket { get; set; }
        public int AuthorUserId { get; set; }
        public User? AuthorUser { get; set; }
        public TicketMessageAuthorRoleEnum AuthorRole { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<TicketAttachment> Attachments { get; set; }
    }
}

