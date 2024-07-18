using Finantech.DTOs.Account;

namespace Finantech.DTOs.Invoice
{
    public class InfoInvoicePaymentResponse
    {
        public int Id { get; set; }
        public double AmountPaid { get; set; }
        public string Description { get; set; }
        public DateTime PaymentDate { get; set; }
        public InfoAccountResponse Account { get; set; }
        public Boolean JustForRecord { get; set; }

    }
}
