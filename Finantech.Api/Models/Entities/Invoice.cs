namespace Finantech.Models.Entities
{
    public class Invoice
    {
        public long Id { get; set; }
        public double TotalAmount { get; set; } = 0;
        public double TotalPaid { get; set; } = 0;
        public bool IsPaid { get; set; } = false;
        public DateTime ClosingDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public long CreditCardId { get; set; }

        public CreditCard CreditCard { get; set; }
        public ICollection<InvoicePayment> InvoicePayments { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}
