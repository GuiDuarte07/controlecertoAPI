namespace ControleCerto.DTOs.User
{
    public class ResetUserDataRequest
    {
        public bool Accounts { get; set; }
        public bool Transactions { get; set; }
        public bool Categories { get; set; }
        public bool CreditCards { get; set; }
        public bool Invoices { get; set; }
        public bool RecurringTransactions { get; set; }
        public bool Notifications { get; set; }
        public bool Transferences { get; set; }
    }
}
