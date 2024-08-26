namespace ControleCerto.DTOs.Account
{
    public class BalanceStatement
    {
        public double Balance { get; set; }
        public double Incomes { get; set; }
        public double Expenses { get; set; }
        public double Invoices { get; set; }

        public BalanceStatement(double balance, double incomes, double expenses, double invoices)
        {
            Balance = balance;
            Incomes = incomes;
            Expenses = expenses;
            Invoices = invoices;
        }
    }
}
