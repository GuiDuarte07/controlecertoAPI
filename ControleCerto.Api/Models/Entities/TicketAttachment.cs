namespace ControleCerto.Models.Entities
{
    public class TicketAttachment
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public Ticket? Ticket { get; set; }
        public long? TicketMessageId { get; set; }
        public TicketMessage? TicketMessage { get; set; }
        public int UploadedByUserId { get; set; }
        public User? UploadedByUser { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long SizeBytes { get; set; }
        public string StorageKey { get; set; }
        public string Url { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

