using AutoMapper;
using Finantech.DTOs.Account;
using Finantech.Models.AppDbContext;
using Finantech.Models.DTOs;
using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Finantech.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        public AccountService(AppDbContext appDbContext, IMapper mapper) 
        { 
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<InfoAccountResponse> CreateAccountAsync(CreateAccountRequest accountRequest)
        {
            var createdAccount = _mapper.Map<Account>(accountRequest);

            var account = await _appDbContext.Accounts.AddAsync(createdAccount);

            if (account.Entity == null)
            {
                throw new Exception("Um erro ocorreu ao criar a conta");
            }

            var infoAccount = _mapper.Map<InfoAccountResponse>(account.Entity);

            return infoAccount;
        }

        public async Task<ICollection<InfoTransactionResponse>> GetMonthTransactions(int userId, int? accountId)
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
                .ToListAsync();

            return allTransactions;
        }

        public async Task<ICollection<InfoTransactionResponse>> GetTransactionsWithPagination(int userId, int? accountId, int pageNumber, int pageSize)
        {
            // Calcula o índice inicial com base no número da página e no tamanho da página
            int startIndex = (pageNumber - 1) * pageSize;

            IQueryable<InfoTransactionResponse> transactionsQuery = _appDbContext.Expenses
                .Where(e => e.Account.UserId == userId)
                .Select(e => new InfoTransactionResponse(e));

            IQueryable<InfoTransactionResponse> incomesQuery = _appDbContext.Incomes
                .Where(i => i.Account.UserId == userId)
                .Select(i => new InfoTransactionResponse(i));

            IQueryable<InfoTransactionResponse> creditExpensesQuery = _appDbContext.CreditExpenses
                .Where(ce => ce.Account.UserId == userId)
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
                .Skip(startIndex) // Pula os registros correspondentes às páginas anteriores
                .Take(pageSize) // Obtém a quantidade de registros correspondente ao tamanho da página
                .ToListAsync();

            return allTransactions;
        }
    }
}
