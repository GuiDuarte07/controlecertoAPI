namespace ControleCerto.Modules.Tickets.DTOs
{
    public class TicketDetailResponse : TicketResponse
    {
        public List<TicketMessageResponse> Messages { get; set; } = new();
        public List<TicketAttachmentResponse> Attachments { get; set; } = new();
    }
}

