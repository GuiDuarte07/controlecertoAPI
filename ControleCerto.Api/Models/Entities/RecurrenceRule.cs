using ControleCerto.Enums;

namespace ControleCerto.Models.Entities
{
    public class RecurrenceRule
    {
        public long Id { get; set; }
        public RecurrenceFrequencyEnum Frequency { get; set; } // DAILY, WEEKLY, MONTHLY, YEARLY
        
        // Para recorrência DIÁRIA
        public bool IsEveryDay { get; set; } = true; // Se false, usa DaysOfWeek
        public string? DaysOfWeek { get; set; } // JSON: 12345 = seg-sex; 135 = seg,qua,sex; 60 = sab, dom
        
        // Para recorrência SEMANAL
        public int? DayOfWeek { get; set; } //  0=domingo, 1=segunda, 2=terça, ..., 6 = sábado

        // Para recorrência MENSAL
        public int? DayOfMonth { get; set; } // 1-28, ou -1 para último dia do mês
        
        // Para recorrência ANUAL
        public int? MonthOfYear { get; set; } // 1-12 (janeiro-dezembro)
        public int? DayOfMonthForYearly { get; set; } // 1-31
        
        // Intervalo ("a cada 2 semanas", "a cada 3 meses")
        public int Interval { get; set; } = 1;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Relacionamento
        public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();
    }
}
