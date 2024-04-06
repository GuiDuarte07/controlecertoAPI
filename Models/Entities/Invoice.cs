namespace Finantech.Models.Entities
{
    public class Invoice
    {
        public int Id { get; set; }
        public double? TotalAmount { get; set; }
        public double? TotalPaid { get; set; }
        public bool? IsPaid { get; set; }
        public DateTime? ClosingDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CreditCardId { get; set; }

        public CreditCard CreditCard { get; set; }
        public ICollection<InvoicePayment> InvoicePayments { get; set; }
        public ICollection<CreditExpense> CreditExpenses { get; set; }
    }
}
