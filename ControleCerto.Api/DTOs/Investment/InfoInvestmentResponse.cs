using System;
using System.Collections.Generic;

namespace ControleCerto.DTOs.Investment
{
    public class InfoInvestmentResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public double CurrentValue { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public IEnumerable<InvestmentHistoryResponse>? Histories { get; set; }
    }
}
