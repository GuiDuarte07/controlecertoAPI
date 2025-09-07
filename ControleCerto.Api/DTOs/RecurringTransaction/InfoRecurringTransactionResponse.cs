using ControleCerto.Enums;

namespace ControleCerto.DTOs.RecurringTransaction
{
    public class InfoRecurringTransactionResponse
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public TransactionTypeEnum Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public long AccountId { get; set; }
        public long CategoryId { get; set; }
        public long RecurrenceRuleId { get; set; }
        public InfoRecurrenceRuleResponse RecurrenceRule { get; set; }
        public int PendingInstancesCount { get; set; }
        public int ConfirmedInstancesCount { get; set; }
        public int RejectedInstancesCount { get; set; }
    }
}
