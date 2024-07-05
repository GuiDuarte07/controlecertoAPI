using Finantech.Enums;
using System.Reflection.Metadata;

namespace Finantech.Models.Entities
{
    public class Transaction
    {
        public long Id { get; set; }
        public TransactionTypeEnum Type { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }
        public String? Observations { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? Destination { get; set; }
        public bool JustForRecord { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public long AccountId { get; set; }
        public Account Account { get; set; }

        public long CategoryId { get; set; }
        public Category Category { get; set; }

        // Credit Card Transaction info:
        public int? InstallmentNumber { get; set; }

        public long? CreditPurchaseId { get; set; }
        public CreditPurchase? CreditPurchase { get; set; }

        public long? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
    }
}
