using AutoMapper;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.DTOs;
using ControleCerto.Models.Entities;
using ControleCerto.Modules.Dashboard.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ControleCerto.Modules.Dashboard.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        public DashboardService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<Result<HomeDashboardResponse>> GetHomeDashboardAsync(int userId, DateTime startDate, DateTime endDate)
        {
            startDate = startDate.ToUniversalTime();
            var adjustedEndDate = endDate.ToUniversalTime().Date.AddDays(1).AddTicks(-1);

            if (startDate > adjustedEndDate)
            {
                return new AppError("Data inicial não pode ser maior que data final.", ErrorTypeEnum.BusinessRule);
            }

            // Buscar contas do usuário
            var accounts = await _appDbContext.Accounts
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var accountIds = accounts.Select(a => a.Id).ToList();
            var hasAccounts = accountIds.Count > 0;

            // Buscar todas as transações do período
            var transactions = hasAccounts
                ? await _appDbContext.Transactions
                    .AsNoTracking()
                    .Include(t => t.Account)
                    .Include(t => t.Category)
                        .ThenInclude(c => c.Parent)
                    .Include(t => t.CreditPurchase)
                    .Where(t => accountIds.Contains(t.AccountId) && t.PurchaseDate >= startDate && t.PurchaseDate <= adjustedEndDate)
                    .OrderByDescending(t => t.PurchaseDate)
                    .ToListAsync()
                : new List<Transaction>();

            // Despesas de cartão devem entrar pelo mês da fatura (InvoiceDate), não pelo PurchaseDate da compra.
            var invoices = hasAccounts
                ? await _appDbContext.Invoices
                    .AsNoTracking()
                    .Include(i => i.Transactions)
                        .ThenInclude(t => t.Category)
                            .ThenInclude(c => c.Parent)
                    .Where(i =>
                        accountIds.Contains(i.CreditCard.AccountId) &&
                        i.InvoiceDate >= startDate &&
                        i.InvoiceDate <= adjustedEndDate)
                    .ToListAsync()
                : new List<Invoice>();

            var transactionAggregates = BuildTransactionAggregates(transactions);
            ApplyInvoiceAggregates(transactionAggregates, invoices);

            var categoryIds = transactionAggregates.Categories.Keys.ToList();
            var activeCategoryLimits = categoryIds.Count > 0
                ? await _appDbContext.CategoryLimits
                    .AsNoTracking()
                    .Where(cl => categoryIds.Contains(cl.CategoryId) && cl.EndDate == null)
                    .GroupBy(cl => cl.CategoryId)
                    .Select(g => new
                    {
                        CategoryId = g.Key,
                        Amount = g.OrderByDescending(cl => cl.StartDate).Select(cl => cl.Amount).FirstOrDefault()
                    })
                    .ToDictionaryAsync(x => x.CategoryId, x => x.Amount)
                : new Dictionary<long, double>();

            // Buscar cartões de crédito do usuário
            var creditCards = hasAccounts
                ? await _appDbContext.CreditCards
                    .AsNoTracking()
                    .Include(c => c.Account)
                    .Where(c => accountIds.Contains(c.AccountId))
                    .ToListAsync()
                : new List<CreditCard>();

            // Buscar investimentos do usuário
            var investments = await _appDbContext.Investments
                .AsNoTracking()
                .Include(i => i.Histories)
                .Where(i => i.UserId == userId)
                .ToListAsync();

            // Calcular resumo financeiro
            var financialSummary = CalculateFinancialSummary(transactionAggregates, accounts, creditCards, investments, startDate, adjustedEndDate);

            // Resumo de contas
            var accountsSummary = CalculateAccountsSummary(accounts, transactionAggregates.Accounts);

            // Resumo de cartões de crédito
            var creditCardsSummary = CalculateCreditCardsSummary(creditCards, transactionAggregates.CreditCards);

            // Gastos por categoria
            var expensesByCategory = CalculateExpensesByCategory(
                transactionAggregates.Categories,
                activeCategoryLimits,
                transactionAggregates.TotalExpense
            );

            // Resumo de investimentos
            var investmentsSummary = CalculateInvestmentsSummary(investments);

            // Balanços diários
            var dailyBalances = CalculateDailyBalances(transactionAggregates.Daily, startDate, adjustedEndDate);

            // Tendências mensais (últimos 12 meses a partir da data final)
            var monthlyTrends = await CalculateMonthlyTrends(accountIds, adjustedEndDate);

            // Transações recentes (últimas 10)
            var recentTransactions = _mapper.Map<List<InfoTransactionResponse>>(
                transactions.Take(10).ToList()
            );

            var response = new HomeDashboardResponse
            {
                StartDate = startDate,
                EndDate = adjustedEndDate,
                FinancialSummary = financialSummary,
                Accounts = accountsSummary,
                CreditCards = creditCardsSummary,
                ExpensesByCategory = expensesByCategory,
                InvestmentsSummary = investmentsSummary,
                DailyBalances = dailyBalances,
                MonthlyTrends = monthlyTrends,
                RecentTransactions = recentTransactions
            };

            return response;
        }

        private FinancialSummary CalculateFinancialSummary(
            TransactionAggregates transactionAggregates,
            List<Account> accounts,
            List<CreditCard> creditCards,
            List<Investment> investments,
            DateTime startDate,
            DateTime endDate)
        {
            var totalIncome = transactionAggregates.TotalIncome;
            var totalExpense = transactionAggregates.TotalExpense;

            var totalAccountsBalance = accounts.Sum(a => a.Balance);
            var totalCreditUsed = creditCards.Sum(c => c.UsedLimit);
            var totalCreditAvailable = creditCards.Sum(c => c.TotalLimit - c.UsedLimit);
            var totalInvestments = investments.Sum(i => i.CurrentValue);

            var days = (endDate.Date - startDate.Date).Days + 1;
            var averageDailyExpense = days > 0 ? totalExpense / days : 0;
            var projectedMonthlyExpense = averageDailyExpense * 30;

            return new FinancialSummary
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                NetBalance = totalIncome - totalExpense,
                TotalAccountsBalance = totalAccountsBalance,
                TotalCreditUsed = totalCreditUsed,
                TotalCreditAvailable = totalCreditAvailable,
                TotalInvestments = totalInvestments,
                TotalTransactions = transactionAggregates.TotalTransactions,
                AverageDailyExpense = averageDailyExpense,
                ProjectedMonthlyExpense = projectedMonthlyExpense
            };
        }

        private List<AccountSummary> CalculateAccountsSummary(
            List<Account> accounts,
            IReadOnlyDictionary<long, AccountAggregate> accountAggregates)
        {
            return accounts.Select(account =>
            {
                accountAggregates.TryGetValue(account.Id, out var aggregate);

                return new AccountSummary
                {
                    Id = account.Id,
                    Bank = account.Bank,
                    Description = account.Description,
                    Color = account.Color,
                    Balance = account.Balance,
                    TotalIncome = aggregate?.TotalIncome ?? 0,
                    TotalExpense = aggregate?.TotalExpense ?? 0,
                    TransactionCount = aggregate?.TransactionCount ?? 0
                };
            })
            .OrderByDescending(a => a.Balance)
            .ToList();
        }

        private List<CreditCardSummary> CalculateCreditCardsSummary(
            List<CreditCard> creditCards,
            IReadOnlyDictionary<long, CreditCardAggregate> creditCardAggregates)
        {
            return creditCards.Select(card =>
            {
                creditCardAggregates.TryGetValue(card.Id, out var aggregate);
                var currentInvoiceAmount = aggregate?.TotalAmount ?? 0;
                var availableLimit = card.TotalLimit - card.UsedLimit;
                var usagePercentage = card.TotalLimit > 0 ? (card.UsedLimit / card.TotalLimit) * 100 : 0;

                return new CreditCardSummary
                {
                    Id = (int)card.Id,
                    Description = card.Description,
                    TotalLimit = card.TotalLimit,
                    UsedLimit = card.UsedLimit,
                    AvailableLimit = availableLimit,
                    UsagePercentage = usagePercentage,
                    CurrentInvoiceAmount = currentInvoiceAmount,
                    TransactionCount = aggregate?.TransactionCount ?? 0,
                    AccountBank = card.Account?.Bank ?? "N/A"
                };
            })
            .OrderByDescending(c => c.UsagePercentage)
            .ToList();
        }

        private List<CategoryExpense> CalculateExpensesByCategory(
            IReadOnlyDictionary<long, CategoryAggregate> categoryAggregates,
            IReadOnlyDictionary<long, double> activeCategoryLimits,
            double totalExpenses)
        {
            return categoryAggregates.Values
                .Select(categoryAggregate =>
                {
                    activeCategoryLimits.TryGetValue(categoryAggregate.CategoryId, out var limitAmount);
                    var categoryLimit = activeCategoryLimits.ContainsKey(categoryAggregate.CategoryId)
                        ? limitAmount
                        : (double?)null;
                    var percentage = totalExpenses > 0 ? (categoryAggregate.TotalAmount / totalExpenses) * 100 : 0;
                    var limitUsagePercentage = categoryLimit.HasValue && categoryLimit.Value > 0
                        ? (categoryAggregate.TotalAmount / categoryLimit.Value) * 100
                        : (double?)null;

                    return new CategoryExpense
                    {
                        CategoryId = categoryAggregate.CategoryId,
                        CategoryName = categoryAggregate.CategoryName,
                        CategoryIcon = categoryAggregate.CategoryIcon,
                        CategoryColor = categoryAggregate.CategoryColor,
                        ParentCategoryId = categoryAggregate.ParentCategoryId,
                        ParentCategoryName = categoryAggregate.ParentCategoryName,
                        TotalAmount = categoryAggregate.TotalAmount,
                        TransactionCount = categoryAggregate.TransactionCount,
                        Percentage = percentage,
                        CategoryLimit = categoryLimit,
                        LimitUsagePercentage = limitUsagePercentage
                    };
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToList();
        }

        private InvestmentsSummary CalculateInvestmentsSummary(List<Investment> investments)
        {
            var totalValue = investments.Sum(i => i.CurrentValue);
            var histories = investments.SelectMany(i => i.Histories ?? Enumerable.Empty<InvestmentHistory>()).ToList();

            var totalDividends = histories
                .Where(h => h.Type == InvestmentHistoryTypeEnum.YIELD)
                .Sum(h => h.ChangeAmount);

            var totalDeposits = histories
                .Where(h => h.Type == InvestmentHistoryTypeEnum.INVEST)
                .Sum(h => Math.Abs(h.ChangeAmount));

            var totalWithdrawals = histories
                .Where(h => h.Type == InvestmentHistoryTypeEnum.WITHDRAW)
                .Sum(h => Math.Abs(h.ChangeAmount));

            var netProfit = totalValue - totalDeposits + totalWithdrawals;

            var investmentDetails = investments.Select(i =>
            {
                var dividends = i.Histories?
                    .Where(h => h.Type == InvestmentHistoryTypeEnum.YIELD)
                    .Sum(h => h.ChangeAmount) ?? 0;

                return new InvestmentDetail
                {
                    Id = i.Id,
                    Name = i.Name,
                    CurrentValue = i.CurrentValue,
                    TotalDividends = dividends,
                    StartDate = i.StartDate
                };
            })
            .OrderByDescending(i => i.CurrentValue)
            .ToList();

            return new InvestmentsSummary
            {
                TotalValue = totalValue,
                TotalInvestments = investments.Count,
                TotalDividends = totalDividends,
                TotalDeposits = totalDeposits,
                TotalWithdrawals = totalWithdrawals,
                NetProfit = netProfit,
                Investments = investmentDetails
            };
        }

        private List<DailyBalance> CalculateDailyBalances(
            IReadOnlyDictionary<DateTime, DailyAggregate> dailyAggregates,
            DateTime startDate,
            DateTime endDate)
        {
            var dailyBalances = new List<DailyBalance>();
            var cumulativeBalance = 0.0;

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                dailyAggregates.TryGetValue(date, out var dayAggregate);

                var income = dayAggregate?.Income ?? 0;
                var expense = dayAggregate?.Expense ?? 0;

                var balance = income - expense;
                cumulativeBalance += balance;

                dailyBalances.Add(new DailyBalance
                {
                    Date = date,
                    Income = income,
                    Expense = expense,
                    Balance = balance,
                    CumulativeBalance = cumulativeBalance
                });
            }

            return dailyBalances;
        }

        private async Task<List<MonthlyTrend>> CalculateMonthlyTrends(List<long> accountIds, DateTime endDate)
        {
            var trendStartDate = endDate.AddMonths(-11).Date;
            trendStartDate = new DateTime(trendStartDate.Year, trendStartDate.Month, 1);

            // Receitas e despesas não-cartão por PurchaseDate.
            var monthlyGroups = accountIds.Count > 0
                ? await _appDbContext.Transactions
                    .AsNoTracking()
                    .Where(t =>
                        accountIds.Contains(t.AccountId) &&
                        t.PurchaseDate >= trendStartDate &&
                        t.PurchaseDate <= endDate &&
                        t.Type != TransactionTypeEnum.CREDITEXPENSE)
                    .GroupBy(t => new { t.PurchaseDate.Year, t.PurchaseDate.Month })
                    .Select(g => new MonthlyTrendAggregate
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalIncome = g.Where(t => t.Type == TransactionTypeEnum.INCOME).Sum(t => t.Amount),
                        TotalExpense = g.Where(t => t.Type == TransactionTypeEnum.EXPENSE).Sum(t => t.Amount),
                        TransactionCount = g.Count()
                    })
                    .ToListAsync()
                : new List<MonthlyTrendAggregate>();

            // Despesas de cartão por mês da fatura.
            var monthlyInvoiceExpenses = accountIds.Count > 0
                ? await _appDbContext.Invoices
                    .AsNoTracking()
                    .Where(i =>
                        accountIds.Contains(i.CreditCard.AccountId) &&
                        i.InvoiceDate >= trendStartDate &&
                        i.InvoiceDate <= endDate)
                    .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
                    .Select(g => new MonthlyInvoiceExpenseAggregate
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalExpense = g.Sum(i => i.TotalAmount)
                    })
                    .ToListAsync()
                : new List<MonthlyInvoiceExpenseAggregate>();

            // Quantidade de transações de cartão no mês da fatura.
            var monthlyCreditTransactionCounts = accountIds.Count > 0
                ? await _appDbContext.Transactions
                    .AsNoTracking()
                    .Where(t =>
                        t.Type == TransactionTypeEnum.CREDITEXPENSE &&
                        accountIds.Contains(t.AccountId) &&
                        t.Invoice != null &&
                        t.Invoice.InvoiceDate >= trendStartDate &&
                        t.Invoice.InvoiceDate <= endDate)
                    .GroupBy(t => new { t.Invoice!.InvoiceDate.Year, t.Invoice.InvoiceDate.Month })
                    .Select(g => new MonthlyCreditTransactionCountAggregate
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TransactionCount = g.Count()
                    })
                    .ToListAsync()
                : new List<MonthlyCreditTransactionCountAggregate>();

            var monthlyGroupByKey = monthlyGroups
                .ToDictionary(m => (m.Year, m.Month));
            var monthlyInvoiceExpenseByKey = monthlyInvoiceExpenses
                .ToDictionary(m => (m.Year, m.Month), m => m.TotalExpense);
            var monthlyCreditTransactionCountByKey = monthlyCreditTransactionCounts
                .ToDictionary(m => (m.Year, m.Month), m => m.TransactionCount);

            // Preencher meses sem transações
            var allMonths = new List<MonthlyTrend>();
            for (var date = trendStartDate; date <= endDate; date = date.AddMonths(1))
            {
                monthlyInvoiceExpenseByKey.TryGetValue((date.Year, date.Month), out var invoiceExpenseForMonth);
                monthlyCreditTransactionCountByKey.TryGetValue((date.Year, date.Month), out var creditTransactionCountForMonth);

                if (monthlyGroupByKey.TryGetValue((date.Year, date.Month), out var existing))
                {
                    var totalExpense = existing.TotalExpense + invoiceExpenseForMonth;
                    var transactionCount = existing.TransactionCount + creditTransactionCountForMonth;

                    allMonths.Add(new MonthlyTrend
                    {
                        Year = existing.Year,
                        Month = existing.Month,
                        MonthName = new DateTime(existing.Year, existing.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture),
                        TotalIncome = existing.TotalIncome,
                        TotalExpense = totalExpense,
                        NetBalance = existing.TotalIncome - totalExpense,
                        TransactionCount = transactionCount
                    });
                }
                else
                {
                    allMonths.Add(new MonthlyTrend
                    {
                        Year = date.Year,
                        Month = date.Month,
                        MonthName = date.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                        TotalIncome = 0,
                        TotalExpense = invoiceExpenseForMonth,
                        NetBalance = -invoiceExpenseForMonth,
                        TransactionCount = creditTransactionCountForMonth
                    });
                }
            }

            return allMonths;
        }

        private static TransactionAggregates BuildTransactionAggregates(IEnumerable<Transaction> transactions)
        {
            var result = new TransactionAggregates();

            foreach (var transaction in transactions)
            {
                result.TotalTransactions++;

                var isIncome = transaction.Type == TransactionTypeEnum.INCOME;
                var isExpense = transaction.Type == TransactionTypeEnum.EXPENSE;

                if (isIncome)
                {
                    result.TotalIncome += transaction.Amount;
                }

                if (isExpense)
                {
                    result.TotalExpense += transaction.Amount;
                }

                if (!result.Accounts.TryGetValue(transaction.AccountId, out var accountAggregate))
                {
                    accountAggregate = new AccountAggregate();
                    result.Accounts[transaction.AccountId] = accountAggregate;
                }

                accountAggregate.TransactionCount++;
                if (isIncome)
                {
                    accountAggregate.TotalIncome += transaction.Amount;
                }
                if (transaction.Type == TransactionTypeEnum.EXPENSE)
                {
                    accountAggregate.TotalExpense += transaction.Amount;
                }

                if (isExpense && transaction.Category is not null)
                {
                    if (!result.Categories.TryGetValue(transaction.CategoryId, out var categoryAggregate))
                    {
                        categoryAggregate = new CategoryAggregate
                        {
                            CategoryId = transaction.Category.Id,
                            CategoryName = transaction.Category.Name,
                            CategoryIcon = transaction.Category.Icon,
                            CategoryColor = transaction.Category.Color,
                            ParentCategoryId = transaction.Category.ParentId,
                            ParentCategoryName = transaction.Category.Parent?.Name,
                        };
                        result.Categories[transaction.CategoryId] = categoryAggregate;
                    }

                    categoryAggregate.TotalAmount += transaction.Amount;
                    categoryAggregate.TransactionCount++;
                }

                var purchaseDay = transaction.PurchaseDate.Date;
                if (!result.Daily.TryGetValue(purchaseDay, out var dailyAggregate))
                {
                    dailyAggregate = new DailyAggregate();
                    result.Daily[purchaseDay] = dailyAggregate;
                }

                if (isIncome)
                {
                    dailyAggregate.Income += transaction.Amount;
                }
                if (isExpense)
                {
                    dailyAggregate.Expense += transaction.Amount;
                }
            }

            return result;
        }

        private static void ApplyInvoiceAggregates(TransactionAggregates result, IEnumerable<Invoice> invoices)
        {
            foreach (var invoice in invoices)
            {
                result.TotalExpense += invoice.TotalAmount;

                if (!result.CreditCards.TryGetValue(invoice.CreditCardId, out var cardAggregate))
                {
                    cardAggregate = new CreditCardAggregate();
                    result.CreditCards[invoice.CreditCardId] = cardAggregate;
                }

                cardAggregate.TotalAmount += invoice.TotalAmount;

                var invoiceTransactions = invoice.Transactions?
                    .Where(t => t.Type == TransactionTypeEnum.CREDITEXPENSE)
                    .ToList() ?? new List<Transaction>();

                cardAggregate.TransactionCount += invoiceTransactions.Count;

                var invoiceDay = invoice.InvoiceDate.Date;
                if (!result.Daily.TryGetValue(invoiceDay, out var dailyAggregate))
                {
                    dailyAggregate = new DailyAggregate();
                    result.Daily[invoiceDay] = dailyAggregate;
                }

                dailyAggregate.Expense += invoice.TotalAmount;

                foreach (var transaction in invoiceTransactions)
                {
                    if (transaction.Category is null)
                    {
                        continue;
                    }

                    if (!result.Categories.TryGetValue(transaction.CategoryId, out var categoryAggregate))
                    {
                        categoryAggregate = new CategoryAggregate
                        {
                            CategoryId = transaction.Category.Id,
                            CategoryName = transaction.Category.Name,
                            CategoryIcon = transaction.Category.Icon,
                            CategoryColor = transaction.Category.Color,
                            ParentCategoryId = transaction.Category.ParentId,
                            ParentCategoryName = transaction.Category.Parent?.Name,
                        };

                        result.Categories[transaction.CategoryId] = categoryAggregate;
                    }

                    categoryAggregate.TotalAmount += transaction.Amount;
                    categoryAggregate.TransactionCount++;
                }
            }
        }

        private sealed class TransactionAggregates
        {
            public int TotalTransactions { get; set; }
            public double TotalIncome { get; set; }
            public double TotalExpense { get; set; }
            public Dictionary<long, AccountAggregate> Accounts { get; } = new();
            public Dictionary<long, CreditCardAggregate> CreditCards { get; } = new();
            public Dictionary<long, CategoryAggregate> Categories { get; } = new();
            public Dictionary<DateTime, DailyAggregate> Daily { get; } = new();
        }

        private sealed class AccountAggregate
        {
            public double TotalIncome { get; set; }
            public double TotalExpense { get; set; }
            public int TransactionCount { get; set; }
        }

        private sealed class CreditCardAggregate
        {
            public double TotalAmount { get; set; }
            public int TransactionCount { get; set; }
        }

        private sealed class CategoryAggregate
        {
            public long CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public string CategoryIcon { get; set; } = string.Empty;
            public string CategoryColor { get; set; } = string.Empty;
            public long? ParentCategoryId { get; set; }
            public string? ParentCategoryName { get; set; }
            public double TotalAmount { get; set; }
            public int TransactionCount { get; set; }
        }

        private sealed class DailyAggregate
        {
            public double Income { get; set; }
            public double Expense { get; set; }
        }

        private sealed class MonthlyTrendAggregate
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public double TotalIncome { get; set; }
            public double TotalExpense { get; set; }
            public int TransactionCount { get; set; }
        }

        private sealed class MonthlyInvoiceExpenseAggregate
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public double TotalExpense { get; set; }
        }

        private sealed class MonthlyCreditTransactionCountAggregate
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public int TransactionCount { get; set; }
        }
    }
}
