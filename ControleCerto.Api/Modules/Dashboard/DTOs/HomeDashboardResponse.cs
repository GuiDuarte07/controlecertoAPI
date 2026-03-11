using ControleCerto.DTOs.Account;
using ControleCerto.DTOs.CreditCard;
using ControleCerto.DTOs.Category;
using ControleCerto.Models.DTOs;

namespace ControleCerto.Modules.Dashboard.DTOs
{
    public class HomeDashboardResponse
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public FinancialSummary FinancialSummary { get; set; }
        public List<AccountSummary> Accounts { get; set; }
        public List<CreditCardSummary> CreditCards { get; set; }
        public List<CategoryExpense> ExpensesByCategory { get; set; }
        public InvestmentsSummary InvestmentsSummary { get; set; }
        public List<DailyBalance> DailyBalances { get; set; }
        public List<MonthlyTrend> MonthlyTrends { get; set; }
        public List<InfoTransactionResponse> RecentTransactions { get; set; }
    }

    public class FinancialSummary
    {
        public double TotalIncome { get; set; }
        public double TotalExpense { get; set; }
        public double NetBalance { get; set; }
        public double TotalAccountsBalance { get; set; }
        public double TotalCreditUsed { get; set; }
        public double TotalCreditAvailable { get; set; }
        public double TotalInvestments { get; set; }
        public int TotalTransactions { get; set; }
        public double AverageDailyExpense { get; set; }
        public double ProjectedMonthlyExpense { get; set; }
    }

    public class AccountSummary
    {
        public long Id { get; set; }
        public string Bank { get; set; }
        public string? Description { get; set; }
        public string Color { get; set; }
        public double Balance { get; set; }
        public double TotalIncome { get; set; }
        public double TotalExpense { get; set; }
        public int TransactionCount { get; set; }
    }

    public class CreditCardSummary
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double TotalLimit { get; set; }
        public double UsedLimit { get; set; }
        public double AvailableLimit { get; set; }
        public double UsagePercentage { get; set; }
        public double CurrentInvoiceAmount { get; set; }
        public int TransactionCount { get; set; }
        public string AccountBank { get; set; }
    }

    public class CategoryExpense
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryIcon { get; set; }
        public string CategoryColor { get; set; }
        public long? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public double TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public double Percentage { get; set; }
        public double? CategoryLimit { get; set; }
        public double? LimitUsagePercentage { get; set; }
    }

    public class InvestmentsSummary
    {
        public double TotalValue { get; set; }
        public int TotalInvestments { get; set; }
        public double TotalDividends { get; set; }
        public double TotalDeposits { get; set; }
        public double TotalWithdrawals { get; set; }
        public double NetProfit { get; set; }
        public List<InvestmentDetail> Investments { get; set; }
    }

    public class InvestmentDetail
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double CurrentValue { get; set; }
        public double TotalDividends { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class DailyBalance
    {
        public DateTime Date { get; set; }
        public double Income { get; set; }
        public double Expense { get; set; }
        public double Balance { get; set; }
        public double CumulativeBalance { get; set; }
    }

    public class MonthlyTrend
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public double TotalIncome { get; set; }
        public double TotalExpense { get; set; }
        public double NetBalance { get; set; }
        public int TransactionCount { get; set; }
    }
}
