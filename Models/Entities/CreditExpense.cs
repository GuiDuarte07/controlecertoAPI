namespace Finantech.Models.Entities
{
    public class CreditExpense
    {
        public int Id { get; set; }
        public double? Amount { get; set; }
        public string Description { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int? InstallmentNumber { get; set; }
        public string Destination { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public int CreditPurchaseId { get; set; }
        public int AccountId { get; set; }
        public int InvoiceId { get; set; }
        public int CategoryId { get; set; }

        public CreditPurchase CreditPurchase { get; set; }
        public Account Account { get; set; }
        public Invoice Invoice { get; set; }
        public Category Category { get; set; }
    }
}
