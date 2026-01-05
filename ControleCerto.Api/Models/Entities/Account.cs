using ControleCerto.Enums;

namespace ControleCerto.Models.Entities
{
    public class Account
    {
        public long Id { get; set; }
        public double Balance { get; set; }
        public string? Description { get; set; }
        public string Bank { get; set; }
        public string Color { get; set; }
        public int UserId { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }

        public User? User { get; set; }
        public CreditCard? CreditCard { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<Transference> Transferences { get; set; }
        public ICollection<InvoicePayment> InvoicePayments { get; set; }
    }
}
