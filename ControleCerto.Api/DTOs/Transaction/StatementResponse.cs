using ControleCerto.Models.DTOs;

namespace ControleCerto.DTOs.Transaction
{
    /// <summary>
    /// Response for statement mode (detailed ledger/full transaction list).
    /// Contains all transactions and invoice payments filtered, sorted, and formatted for detailed analysis.
    /// </summary>
    public class StatementResponse
    {
        /// <summary>
        /// Filter metadata: start date used for the query
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Filter metadata: end date used for the query
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// List of all transactions (cash + credit purchases + invoice payments) ordered and filtered
        /// </summary>
        public List<InfoTransactionResponse> Transactions { get; set; } = new();

        /// <summary>
        /// Summary statistics of the returned transactions
        /// </summary>
        public StatementSummary Summary { get; set; } = new();
    }

    /// <summary>
    /// Summary statistics for statement transactions
    /// </summary>
    public class StatementSummary
    {
        /// <summary>
        /// Total count of transactions in the statement
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Sum of all income transactions
        /// </summary>
        public double TotalIncome { get; set; }

        /// <summary>
        /// Sum of all expense transactions (cash)
        /// </summary>
        public double TotalExpense { get; set; }

        /// <summary>
        /// Sum of all credit card purchases
        /// </summary>
        public double TotalCreditCharges { get; set; }

        /// <summary>
        /// Sum of all invoice payments
        /// </summary>
        public double TotalInvoicePayments { get; set; }

        /// <summary>
        /// Net result = Income - Expense - CreditCharges
        /// </summary>
        public double NetBalance { get; set; }
    }
}
