namespace Finantech.Models.Entities
{
    public class CreditCard
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public double? Limit { get; set; }
        public string Description { get; set; }
        public string CardBrand { get; set; }
        public string CreditType { get; set; }
        public Account AccountId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? UpdatedAt { get; set; }
        public Account Account { get; set; }
        public ICollection<Invoice> Invoices { get; set; }
        public ICollection<CreditPurchase> CreditPurchases { get; set; }
    }
}
