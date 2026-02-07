using System;

namespace ControleCerto.DTOs.Investment
{
    public class DepositInvestmentRequest
    {
        public long InvestmentId { get; set; }
        public double Amount { get; set; }
        public long? AccountId { get; set; }
        public DateTime? OccurredAt { get; set; }
        public string? Note { get; set; }
    }
}
