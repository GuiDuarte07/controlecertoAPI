using AutoMapper;
using Finantech.DTOs.Account;
using Finantech.Enums;
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

        public async Task<InfoAccountResponse> CreateAccountAsync(CreateAccountRequest accountRequest, int userId)
        {
            var createdAccount = _mapper.Map<Account>(accountRequest);
            createdAccount.UserId = userId;

            var account = await _appDbContext.Accounts.AddAsync(createdAccount);

            if (account.Entity == null)
            {
                throw new Exception("Um erro ocorreu ao criar a conta");
            }

            await _appDbContext.SaveChangesAsync();

            var infoAccount = _mapper.Map<InfoAccountResponse>(account.Entity);

            return infoAccount;
        }

        public async Task<ICollection<InfoAccountResponse>> GetAccountsByUserIdAsync(int userId)
        {
            var accounts = await _appDbContext.Accounts.Where(a => a.UserId == userId).ToListAsync();

            var accountsInfo = _mapper.Map<List<InfoAccountResponse>>(accounts);

            return accountsInfo;
        }

        public async Task<ICollection<InfoTransactionResponse>> GetMonthTransactionsAsync(int userId, int? accountId)
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

        public async Task<ICollection<InfoTransactionResponse>> GetTransactionsWithPaginationAsync(int pageNumber, int pageSize, int userId, int? accountId)
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

            if (accountId.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.AccountId == accountId);
                incomesQuery = incomesQuery.Where(t => t.AccountId == accountId);
                creditExpensesQuery = creditExpensesQuery.Where(t => t.AccountId == accountId);
            }

            var allTransactions = await transactionsQuery
                .Concat(incomesQuery)
                .Concat(creditExpensesQuery)
                .OrderByDescending(t => t.PurchaseDate) // Ordena as transações pela data de compra em ordem decrescente
                .Skip(startIndex) // Pula os registros correspondentes às páginas anteriores
                .Take(pageSize) // Obtém a quantidade de registros correspondente ao tamanho da página
                .ToListAsync();

            return allTransactions;
        }

        public async Task<InfoAccountResponse> UpdateAccountAsync(UpdateAccountRequest request, int userId)
        {
            var account = await _appDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == request.Id) ?? throw new Exception("Conta não encontrada.");

            if (account.UserId != userId)
            {
                throw new Exception("Não autorizado: Conta não pertence a usuário.");
            }

            if (request.Description != null)
                account.Description = request.Description;
            if (request.Bank != null)
                account.Bank = request.Bank;
            if (request.AccountType != null)
                account.AccountType = (AccountTypeEnum)request.AccountType;
            if (request.Color != null)
                account.Color = request.Color;

            var updatedAccount = _appDbContext.Accounts.Update(account);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoAccountResponse>(updatedAccount);
        }
    }
}
