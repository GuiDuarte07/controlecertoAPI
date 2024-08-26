using ControleCerto.DTOs.Invoice;
using ControleCerto.Models.DTOs;

namespace ControleCerto.DTOs.Transaction
{
    public class TransactionList
    {
        public List<InfoTransactionResponse> Transactions { get; set; }
        public List<InfoInvoiceResponse> Invoices { get; set; }
    }
}
