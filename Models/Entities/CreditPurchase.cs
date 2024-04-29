namespace Finantech.Models.Entities
{
    public class CreditPurchase
    {
        public int Id { get; set; }
        public double TotalAmount { get; set; }
        public int TotalInstallment { get; set; }
        public int InstallmentsPaid { get; set; } = 0;
        public DateTime PurchaseDate { get; set; }
        public bool Paid { get; set; } = false;
        public string Destination { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public int CreditCardId { get; set; }
        public CreditCard CreditCard { get; set; }
        public ICollection<CreditExpense> CreditExpenses { get; set; }
    }
}
