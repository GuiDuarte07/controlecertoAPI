namespace Finantech.DTOs.Invoice
{
    public class InfoInvoicePurchaseResponse
    {
        public int Id { get; set; }
        public double AmountPaid { get; set; }
        public string? Description { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
