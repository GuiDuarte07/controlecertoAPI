using ControleCerto.Enums;

namespace ControleCerto.Models.Entities
{
    public class RecurringTransactionInstance
    {
        public long Id { get; set; }
        public long RecurringTransactionId { get; set; }
        public RecurringTransaction RecurringTransaction { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public InstanceStatusEnum Status { get; set; }
        public long? ActualTransactionId { get; set; }
        public Transaction? ActualTransaction { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
