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

    }
}
