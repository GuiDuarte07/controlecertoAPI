namespace ControleCerto.Modules.Tickets.DTOs
{
    public class TicketAttachmentResponse
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public long? TicketMessageId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long SizeBytes { get; set; }
        public string Url { get; set; }
        public string StorageKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

