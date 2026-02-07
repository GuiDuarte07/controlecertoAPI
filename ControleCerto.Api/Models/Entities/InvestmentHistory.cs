using System;
using ControleCerto.Enums;

namespace ControleCerto.Models.Entities
{
    public class InvestmentHistory
    {
        public long Id { get; set; }
        public long InvestmentId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public double ChangeAmount { get; set; }
        public double TotalValue { get; set; }
        public string? Note { get; set; }
        public long? SourceAccountId { get; set; }
        public InvestmentHistoryTypeEnum Type { get; set; }

        public Investment? Investment { get; set; }
        public Account? SourceAccount { get; set; }
    }
}
