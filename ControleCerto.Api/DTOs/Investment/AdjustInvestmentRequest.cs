using System;

namespace ControleCerto.DTOs.Investment
{
    public class AdjustInvestmentRequest
    {
        public long InvestmentId { get; set; }
        public double NewTotalValue { get; set; }
        public DateTime? OccurredAt { get; set; }
        public string? Note { get; set; }
    }
}
