using Finantech.DTOs.Invoice;
using Finantech.Models.DTOs;

namespace Finantech.DTOs.Transaction
{
    public class TransactionList
    {
        public List<InfoTransactionResponse> Transactions { get; set; }
        public List<InfoInvoiceResponse> Invoices { get; set; }
    }
}
