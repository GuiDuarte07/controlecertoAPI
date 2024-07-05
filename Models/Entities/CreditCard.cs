namespace Finantech.Models.Entities
{
    public class CreditCard
    {
        public long Id { get; set; }
        public double TotalLimit { get; set; }
        public double UsedLimit { get; set; }
        public string Description { get; set; }
        public string CardBrand { get; set; }
        public int DueDay { get; set; }
        public int CloseDay { get; set; }
        public long AccountId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Account Account { get; set; }
        public ICollection<Invoice> Invoices { get; set; }
        public ICollection<CreditPurchase> CreditPurchases { get; set; }
    }
}
