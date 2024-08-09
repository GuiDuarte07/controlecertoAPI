using Finantech.DTOs.CreditCard;
using Finantech.Models.DTOs;

namespace Finantech.DTOs.Invoice
{
    public class InfoInvoiceResponse
    {
        public int Id { get; set; }
        public double TotalAmount { get; set; }
        public double TotalPaid { get; set; }
        public bool IsPaid { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public DateTime DueDate { get; set; }

        public InfoCreditCardResponse CreditCard { get; set; }

        public ICollection<InfoTransactionResponse>? Transactions { get; set; }
        public ICollection<InfoInvoicePaymentResponse>? InvoicePayments { get; set; }
    }
}
