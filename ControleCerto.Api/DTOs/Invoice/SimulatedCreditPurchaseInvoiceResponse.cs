namespace ControleCerto.DTOs.Invoice
{
    public class SimulatedCreditPurchaseInvoiceResponse
    {
        public string CreditCardDescription { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public DateTime DueDate { get; set; }
        public double TotalAmount { get; set; }
    }
}
