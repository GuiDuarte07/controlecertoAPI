namespace ControleCerto.DTOs.User
{
    public class ResetUserDataResponse
    {
        public int AccountsDeleted { get; set; }
        public int CategoriesDeleted { get; set; }
        public int CreditCardsDeleted { get; set; }
        public int CreditPurchasesDeleted { get; set; }
        public int InvoicesDeleted { get; set; }
        public int TransactionsDeleted { get; set; }
        public int RecurringTransactionsDeleted { get; set; }
    }
}
