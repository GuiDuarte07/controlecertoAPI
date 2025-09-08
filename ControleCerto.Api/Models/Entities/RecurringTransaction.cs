using ControleCerto.Enums;

namespace ControleCerto.Models.Entities
{
    public class RecurringTransaction
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public string? Destination { get; set; }
        public bool JustForRecord { get; set; }
        public double Amount { get; set; }
        public TransactionTypeEnum Type { get; set; } // Apenas EXPENSE ou INCOME
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Relacionamentos
        public long AccountId { get; set; }
        public Account Account { get; set; }
        public long CategoryId { get; set; }
        public Category Category { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        
        // Relacionamento com a regra de recorrência
        public long RecurrenceRuleId { get; set; }
        public RecurrenceRule RecurrenceRule { get; set; }
        
        // Para controle de confirmação
        public ICollection<RecurringTransactionInstance> Instances { get; set; } = new List<RecurringTransactionInstance>();
    }
}
