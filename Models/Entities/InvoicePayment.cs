namespace Finantech.Models.Entities
{
    public class InvoicePayment
    {
        public int Id { get; set; }
        public double AmountPaid { get; set; }
        public string Description { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int InvoiceId { get; set; }

        public Invoice Invoice { get; set; }
    }
}
