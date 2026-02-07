using System;
using ControleCerto.DTOs.Account;

namespace ControleCerto.DTOs.Investment
{
    public class InvestmentHistoryResponse
    {
        public long Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public double ChangeAmount { get; set; }
        public double TotalValue { get; set; }
        public string? Note { get; set; }
        public long? SourceAccountId { get; set; }
        public InfoAccountResponse? SourceAccount { get; set; }
        public string Type { get; set; }
    }
}
