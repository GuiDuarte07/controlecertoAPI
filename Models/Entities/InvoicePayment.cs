namespace Finantech.Models.Entities
{
    public class InvoicePayment
    {
        public long Id { get; set; }
        public double AmountPaid { get; set; }
        public string Description { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long InvoiceId { get; set; }
        public long? AccountPaidId { get; set; }
        public Boolean JustForRecord { get; set; } = false;
        public Account? PaidAccount { get; set; }
        public Invoice Invoice { get; set; }
    }
}
