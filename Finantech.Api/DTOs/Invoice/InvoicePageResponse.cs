namespace Finantech.DTOs.Invoice
{
    public class InvoicePageResponse
    {
        public InfoInvoiceResponse Invoice {  get; set; }
        public long? NextInvoiceId { get; set; }
        public long? PrevInvoiceId { get; set; }

        public InvoicePageResponse(InfoInvoiceResponse invoice, long? nextInvoiceId, long? prevInvoiceId)
        {
            Invoice = invoice;
            NextInvoiceId = nextInvoiceId;
            PrevInvoiceId = prevInvoiceId;
        }
    }
}
