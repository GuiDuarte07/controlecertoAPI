using ControleCerto.DTOs.Invoice;
using ControleCerto.Models.DTOs;

namespace ControleCerto.DTOs.Transaction
{
    /// <summary>
    /// Resposta do modo "invoice" com transações e faturas do mês.
    /// </summary>
    public class InvoiceListResponse
    {
        /// <summary>
        /// Lista de transações do mês (exceto despesas de crédito)
        /// </summary>
        public List<InfoTransactionResponse> Transactions { get; set; } = new();

        /// <summary>
        /// Lista de faturas de cartão de crédito do mês
        /// </summary>
        public List<InfoInvoiceResponse> Invoices { get; set; } = new();
    }
}
