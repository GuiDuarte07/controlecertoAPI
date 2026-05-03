using ControleCerto.Enums;

namespace ControleCerto.Modules.Tickets.DTOs
{
    public class TicketMessageResponse
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public int AuthorUserId { get; set; }
        public TicketMessageAuthorRoleEnum AuthorRole { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TicketAttachmentResponse> Attachments { get; set; } = new();
    }
}

