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

        public async Task DeleteAccountAsync(int accountId, int userId)
        {
            var accountToDelete = await _appDbContext.Accounts.Include(a => a.CreditCard).Include(a => a.Incomes).Include(a => a.Expenses).Include(a => a.Transferences).Include(a => a.CreditCard).FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId) ?? throw new Exception("Conta não encontrada.");

            if(accountToDelete.CreditCard is not null)
            {
                throw new Exception("Conta não pode ser deletada pois possui cartão de crédito.");
            }

            if (IsWithin24Hours(accountToDelete.CreatedAt, DateTime.Now) && !accountToDelete.Incomes.Any() && !accountToDelete.Expenses.Any() && !accountToDelete.CreditExpenses.Any() && !accountToDelete.Transferences.Any())
            {
                _appDbContext.Accounts.Remove(accountToDelete);
                await _appDbContext.SaveChangesAsync();
            }
            else
            {
                accountToDelete.Deleted = true;
                _appDbContext.Accounts.Update(accountToDelete);
                await _appDbContext.SaveChangesAsync();
            }

            return;
        }

        public async Task<ICollection<InfoAccountResponse>> GetAccountsByUserIdAsync(int userId)
        {
            var accounts = await _appDbContext.Accounts.Where(a => a.UserId == userId && a.Deleted == false).ToListAsync();

            var accountsInfo = _mapper.Map<List<InfoAccountResponse>>(accounts);

            return accountsInfo;
        }

        public async Task<BalanceStatement> GetBalanceStatementAsync(int userId, DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null) 
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }

            if (endDate == null)
            {
                endDate = DateTime.Now;
            }

            double balance = await _appDbContext.Accounts.Where(a => a.UserId == userId).SumAsync(a => a.Balance);

            double expenses = await _appDbContext.Expenses.Where(e => e.Account!.UserId == userId && e.PurchaseDate >= startDate && e.PurchaseDate <= endDate).SumAsync(e => e.Amount);

            double incomes = await _appDbContext.Incomes.Where(i => i.Account!.UserId == userId && i.PurchaseDate >= startDate && i.PurchaseDate <= endDate).SumAsync(i => i.Amount);

            double invoices = await _appDbContext.Invoices.Where(i => i.CreditCard.Account.UserId == userId && i.ClosingDate >= startDate && i.ClosingDate <= endDate).SumAsync(i => i.TotalAmount - i.TotalPaid);

            DateTime now = DateTime.Now;
            DateTime firstDayNextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
            DateTime lastDayNextMonth = firstDayNextMonth.AddMonths(1).AddDays(-1);
            var invoicesToTestttt = await _appDbContext.Invoices.Where(i => i.CreditCard.Account.UserId == userId && i.ClosingDate > now).Include(i => i.CreditExpenses).ToListAsync();

            return new BalanceStatement(balance, incomes, expenses, invoices);
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

            return _mapper.Map<InfoAccountResponse>(updatedAccount.Entity);
        }

        private bool IsWithin24Hours(DateTime deadline, DateTime currentDate)
        {
            TimeSpan difference = deadline - currentDate;
            return difference.TotalHours <= 24 && difference.TotalHours >= 0;
        }

    }
}
