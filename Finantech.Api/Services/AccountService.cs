using AutoMapper;
using Finantech.DTOs.Account;
using Finantech.Enums;
using Finantech.Errors;
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

        public async Task<Result<InfoAccountResponse>> CreateAccountAsync(CreateAccountRequest accountRequest, int userId)
        {
            var createdAccount = _mapper.Map<Account>(accountRequest);
            createdAccount.UserId = userId;

            var account = await _appDbContext.Accounts.AddAsync(createdAccount);

            if (account.Entity == null)
            {
                return new AppError("Um erro ocorreu ao criar a conta", ErrorTypeEnum.InternalError);
            }

            await _appDbContext.SaveChangesAsync();

            var infoAccount = _mapper.Map<InfoAccountResponse>(account.Entity);

            return infoAccount;
        }

        public async Task<Result<bool>> DeleteAccountAsync(int accountId, int userId)
        {
            var accountToDelete = await _appDbContext.Accounts.Include(a => a.CreditCard).Include(a => a.Transactions).Include(a => a.Transferences).Include(a => a.CreditCard).FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId) ?? null;

            if (accountToDelete is null)
            {
                return new AppError("Conta não encontrada.", ErrorTypeEnum.Validation);
            }

            if(accountToDelete.CreditCard is not null)
            {
                return new AppError("Conta não pode ser deletada pois possui cartão de crédito.", ErrorTypeEnum.BusinessRule);
            }

            if (IsWithin24Hours(accountToDelete.CreatedAt, DateTime.UtcNow) && !accountToDelete.Transactions.Any() && !accountToDelete.Transferences.Any())
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

            return true;
        }

        public async Task<Result<ICollection<InfoAccountResponse>>> GetAccountsByUserIdAsync(int userId)
        {
            var accounts = await _appDbContext.Accounts.Where(a => a.UserId == userId && a.Deleted == false).ToListAsync();

            var accountsInfo = _mapper.Map<List<InfoAccountResponse>>(accounts);

            return accountsInfo;
        }

        public async Task<Result<ICollection<InfoAccountResponse>>> GetAccountsWithoutCreditCardAsync(int userId)
        {
            var accounts = await _appDbContext.Accounts.Where(a => a.UserId == userId && a.Deleted == false && a.CreditCard == null).ToListAsync() ?? [];

            var accountsInfo = _mapper.Map<List<InfoAccountResponse>>(accounts);

            return accountsInfo;
        }

        public async Task<Result<BalanceStatement>> GetBalanceStatementAsync(int userId, DateTime? startDate, DateTime? endDate)
        {
            DateTime currentUtc = DateTime.UtcNow;
            if (startDate == null) 
            {
                startDate = new DateTime(currentUtc.Year, currentUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            }

            if (endDate == null)
            {
                endDate = new DateTime(currentUtc.Year, currentUtc.Month+1, 1, 0, 0, 0, DateTimeKind.Utc);
            }

            double balance = await _appDbContext.Accounts.Where(a => a.UserId == userId && a.Deleted == false).SumAsync(a => a.Balance);

            double expenses = await _appDbContext.Transactions.Where(t => t.Type == TransactionTypeEnum.EXPENSE &&  t.Account!.UserId == userId && t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).SumAsync(e => e.Amount);

            double incomes = await _appDbContext.Transactions.Where(t => t.Type == TransactionTypeEnum.INCOME && t.Account!.UserId == userId && t.PurchaseDate >= startDate && t.PurchaseDate <= endDate).SumAsync(i => i.Amount);

            double invoices = await _appDbContext.Invoices.Where(i => i.CreditCard.Account.UserId == userId && i.ClosingDate >= startDate && i.ClosingDate <= endDate).SumAsync(i => i.TotalAmount - i.TotalPaid);

            return new BalanceStatement(balance, incomes, expenses, invoices);
        }

        public async Task<Result<double>> GetAccountBalanceAsync(int userId)
        {
            return await _appDbContext.Accounts.Where(a => a.UserId == userId && a.Deleted == false).SumAsync(a => a.Balance);
        }

        public async Task<Result<InfoAccountResponse>> UpdateAccountAsync(UpdateAccountRequest request, int userId)
        {
            var account = await _appDbContext.Accounts.FirstAsync(a => a.Id == request.Id);

            if (account is null || account.UserId != userId)
            {
                return new AppError("Conta não encontrada.", ErrorTypeEnum.Validation);
            }

            if (request.Description != null)
                account.Description = request.Description;
            if (request.Bank != null)
                account.Bank = request.Bank;
            if (request.Color != null)
                account.Color = request.Color;

            var updatedAccount = _appDbContext.Accounts.Update(account);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoAccountResponse>(updatedAccount.Entity);
        }

        private bool IsWithin24Hours(DateTime deadline, DateTime currentDate)
        {
            TimeSpan difference = currentDate - deadline;
            return difference.TotalHours <= 24 && difference.TotalHours >= 0;
        }

    }
}
