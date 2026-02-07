using System;
using ControleCerto.Enums;

namespace ControleCerto.Models.Entities
{
    public class Investment
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public double CurrentValue { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public ICollection<InvestmentHistory> Histories { get; set; }
    }
}
