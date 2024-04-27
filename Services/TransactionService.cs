using AutoMapper;
using Finantech.DTOs.Account;
using Finantech.DTOs.Expense;
using Finantech.DTOs.Income;
using Finantech.Enums;
using Finantech.Models.AppDbContext;
using Finantech.Models.DTOs;
using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Finantech.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        public TransactionService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<InfoExpenseResponse> CreateExpenseAsync(CreateExpenseRequest request, int userId)
        {
            bool justForRecord = request.JustForRecord;
            var expenseToCreate = _mapper.Map<Expense>(request);

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!justForRecord) {
                        var account = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == expenseToCreate.AccountId) ?? throw new Exception("Conta não encontrada.");

                        if (account.Balance < expenseToCreate.Amount)
                        {
                            throw new Exception("Valor em conta é menor que o valor da transação.");
                        }

                        account.Balance -= expenseToCreate.Amount;

                        _appDbContext.Update(account);

                        await _appDbContext.SaveChangesAsync();
                    }

                    var expense = await _appDbContext.Expenses.AddAsync(expenseToCreate);

                    await _appDbContext.SaveChangesAsync();

                    transaction.Commit();

                    return _mapper.Map<InfoExpenseResponse>(expense.Entity);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

        }

        public async Task DeleteExpenseAsync(int expenseId, int userId)
        {
            var expenseToDelete = await _appDbContext.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId) ?? throw new Exception("Transação não encontrada");
            bool justForRecord = expenseToDelete.JustForRecord;

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!justForRecord)
                    {
                        var account = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == expenseToDelete.AccountId) ?? throw new Exception("Conta não encontrada.");

                        account.Balance += expenseToDelete.Amount;

                        _appDbContext.Update(account);

                        await _appDbContext.SaveChangesAsync();
                    }

                    var expense = _appDbContext.Expenses.Remove(expenseToDelete);

                    await _appDbContext.SaveChangesAsync();

                    transaction.Commit();

                    return;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<InfoExpenseResponse> UpdateExpenseAsync(UpdateExpenseRequest request, int userId)
        {
            var expenseToUpdate = await _appDbContext.Expenses.Include(e => e.Account).FirstOrDefaultAsync(e => e.Id == request.Id) ?? throw new Exception("Conta não encontrada.");

            expenseToUpdate.UpdatedAt = DateTime.Now;

            if (expenseToUpdate.Account!.UserId != userId)
            {
                throw new Exception("Não autorizado: Transação não pertence a usuário.");
            }

            if (request.Description is not null)
                expenseToUpdate.Description = request.Description;
            if (request.Destination is not null)
                expenseToUpdate.Destination = request.Destination;
            if (request.ExpenseType is not null)
                expenseToUpdate.ExpenseType = (ExpenseTypeEnum)request.ExpenseType;
            if (request.PurchaseDate is not null)
                expenseToUpdate.PurchaseDate = (DateTime)request.PurchaseDate;

            if (request.CategoryId is not null)
            {
                var category = await _appDbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId) 
                    ?? throw new Exception("Nova categória não encntrada.");

                expenseToUpdate.CategoryId = category.Id;
            }
                

            var updatedexpense = _appDbContext.Expenses.Update(expenseToUpdate);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoExpenseResponse>(updatedexpense.Entity);
        }



        public async Task<InfoIncomeResponse> CreateIncomeAsync(CreateIncomeRequest request, int userId)
        {
            bool justForRecord = request.JustForRecord;
            var incomeToCreate = _mapper.Map<Income>(request);

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!justForRecord)
                    {
                        var account = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == incomeToCreate.AccountId) ?? throw new Exception("Conta não encontrada.");

                        account.Balance += incomeToCreate.Amount;

                        _appDbContext.Update(account);

                        await _appDbContext.SaveChangesAsync();
                    }

                    var income = await _appDbContext.Incomes.AddAsync(incomeToCreate);

                    await _appDbContext.SaveChangesAsync();

                    transaction.Commit();

                    return _mapper.Map<InfoIncomeResponse>(income.Entity);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

        }

        public async Task DeleteIncomeAsync(int incomeId, int userId)
        {
            var incomeToDelete = await _appDbContext.Incomes.FirstOrDefaultAsync(e => e.Id == incomeId) ?? throw new Exception("Transação não encontrada");
            bool justForRecord = incomeToDelete.JustForRecord;

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!justForRecord)
                    {
                        var account = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == incomeToDelete.AccountId) ?? throw new Exception("Conta não encontrada.");

                        if (account.Balance < incomeToDelete.Amount)
                        {
                            throw new Exception("Valor em conta é menor que o valor da transação a ser deletado.");
                        }
                        account.Balance -= incomeToDelete.Amount;

                        _appDbContext.Update(account);

                        await _appDbContext.SaveChangesAsync();
                    }

                    var income = _appDbContext.Incomes.Remove(incomeToDelete);

                    await _appDbContext.SaveChangesAsync();

                    transaction.Commit();

                    return;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<InfoIncomeResponse> UpdateIncomeAsync(UpdateIncomeRequest request, int userId)
        {
            var incomeToUpdate = await _appDbContext.Incomes.Include(e => e.Account).FirstOrDefaultAsync(e => e.Id == request.Id) ?? throw new Exception("Conta não encontrada.");

            incomeToUpdate.UpdatedAt = DateTime.Now;

            if (incomeToUpdate.Account!.UserId != userId)
            {
                throw new Exception("Não autorizado: Transação não pertence a usuário.");
            }

            if (request.Description is not null)
                incomeToUpdate.Description = request.Description;
            if (request.Origin is not null)
                incomeToUpdate.Origin = request.Origin;
            if (request.IncomeType is not null)
                incomeToUpdate.IncomeType = (IncomeTypeEnum)request.IncomeType;
            if (request.PurchaseDate is not null)
                incomeToUpdate.PurchaseDate = (DateTime)request.PurchaseDate;

            if (request.CategoryId is not null)
            {
                var category = await _appDbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId)
                    ?? throw new Exception("Nova categória não encntrada.");

                incomeToUpdate.CategoryId = category.Id;
            }


            var updatedIncome = _appDbContext.Incomes.Update(incomeToUpdate);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoIncomeResponse>(updatedIncome.Entity);
        }



        public async Task<IEnumerable<InfoTransactionResponse>> GetTransactionsWithPaginationAsync(int pageNumber, int pageSize, int userId, DateTime startDate, DateTime endDate, int? accountId)
        {
            // Calcula o índice inicial com base no número da página e no tamanho da página
            int startIndex = (pageNumber - 1) * pageSize;

            IQueryable<InfoTransactionResponse> transactionsQuery = _appDbContext.Expenses
                .Include(e => e.Category)
                .Where(e => 
                    e.Account!.UserId == userId && 
                    e.PurchaseDate >= startDate &&
                    e.PurchaseDate <= endDate)
                .Select(e => new InfoTransactionResponse(e));

            IQueryable<InfoTransactionResponse> incomesQuery = _appDbContext.Incomes
                .Include(e => e.Category)
                .Where(i => i.Account.UserId == userId &&
                    i.Account!.UserId == userId &&
                    i.PurchaseDate >= startDate &&
                    i.PurchaseDate <= endDate)
                .Select(i => new InfoTransactionResponse(i));

            IQueryable<InfoTransactionResponse> creditExpensesQuery = _appDbContext.CreditExpenses
                .Include(e => e.Category)
                .Where(ce => ce.Account.UserId == userId &&
                    ce.Account!.UserId == userId &&
                    ce.PurchaseDate >= startDate &&
                    ce.PurchaseDate <= endDate)
                .Select(ce => new InfoTransactionResponse(ce));

            var allTransactions = transactionsQuery.AsEnumerable()
                .Union(incomesQuery.AsEnumerable())
                .Union(creditExpensesQuery.AsEnumerable())
                .OrderByDescending(t => t.PurchaseDate)
                .Skip(startIndex)
                .Take(pageSize);

            if (accountId.HasValue)
            {

                var _ = await _appDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId) ??
                    throw new Exception("Conta não encontrada.");

                allTransactions = allTransactions.Where(t => t.AccountId == accountId);
            }

            return allTransactions;
        }
    }
}


/*public async Task<ICollection<InfoTransactionResponse>> GetMonthTransactionsAsync(int userId, int? accountId)
{
    var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

    IQueryable<InfoTransactionResponse> transactionsQuery = _appDbContext.Expenses
        .Where(e => e.Account.UserId == userId &&
                    e.PurchaseDate >= firstDayOfMonth &&
                    e.PurchaseDate <= lastDayOfMonth)
        .Select(e => new InfoTransactionResponse(e));

    IQueryable<InfoTransactionResponse> incomesQuery = _appDbContext.Incomes
        .Where(i => i.Account.UserId == userId &&
                    i.PurchaseDate >= firstDayOfMonth &&
                    i.PurchaseDate <= lastDayOfMonth)
        .Select(i => new InfoTransactionResponse(i));

    IQueryable<InfoTransactionResponse> creditExpensesQuery = _appDbContext.CreditExpenses
        .Where(ce => ce.Account.UserId == userId &&
                     ce.PurchaseDate >= firstDayOfMonth &&
                     ce.PurchaseDate <= lastDayOfMonth)
        .Select(ce => new InfoTransactionResponse(ce));

    // Se accountId for especificado, filtra as transações pela conta
    if (accountId.HasValue)
    {
        transactionsQuery = transactionsQuery.Where(t => t.AccountId == accountId);
        incomesQuery = incomesQuery.Where(t => t.AccountId == accountId);
        creditExpensesQuery = creditExpensesQuery.Where(t => t.AccountId == accountId);
    }

    // Concatena todas as transações
    var allTransactions = await transactionsQuery
        .Concat(incomesQuery)
        .Concat(creditExpensesQuery)
        .OrderByDescending(t => t.PurchaseDate) // Ordena as transações pela data de compra em ordem decrescente
        .ToListAsync();

    return allTransactions;
}*/