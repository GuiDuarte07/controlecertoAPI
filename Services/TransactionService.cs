using AutoMapper;
using Finantech.DTOs.Account;
using Finantech.DTOs.Expense;
using Finantech.DTOs.Income;
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
                            throw new Exception("Valor é conta é menor que o valor da transação");
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

        public Task<InfoIncomeResponse> CreateIncomeAsync(CreateIncomeRequest request, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<InfoTransactionResponse>> GetMonthTransactionsAsync(int userId, int? accountId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<InfoTransactionResponse>> GetTransactionsWithPaginationAsync(int pageNumber, int pageSize, int userId, int? accountId)
        {
            throw new NotImplementedException();
        }

        public Task<InfoExpenseResponse> UpdateExpenseAsync(UpdateExpenseRequest request, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<InfoIncomeResponse> UpdateIncomeAsync(UpdateIncomeRequest request, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
