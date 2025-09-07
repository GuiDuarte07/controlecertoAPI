using ControleCerto.Enums;

namespace ControleCerto.DTOs.RecurringTransaction
{
    public class InfoRecurringTransactionInstanceResponse
    {
        public long Id { get; set; }
        public long RecurringTransactionId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public InstanceStatusEnum Status { get; set; }
        public long? ActualTransactionId { get; set; }
        public string? RejectionReason { get; set; }
        
        // Informações da transação recorrente
        public string RecurringTransactionDescription { get; set; }
        public double RecurringTransactionAmount { get; set; }
        public TransactionTypeEnum RecurringTransactionType { get; set; }
    }
}
