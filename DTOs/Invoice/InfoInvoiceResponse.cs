namespace Finantech.DTOs.Invoice
{
    public class InfoInvoiceResponse
    {
        public int Id { get; set; }
        public double TotalAmount { get; set; }
        public double TotalPaid { get; set; }
        public bool IsPaid { get; set; }
        public DateTime ClosingDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}
