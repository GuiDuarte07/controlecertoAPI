using AutoMapper;
using Finantech.DTOs.Invoice;
using Finantech.DTOs.Transaction;
using Finantech.DTOs.TransferenceDTO;
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

        /*
         * ========= TRANSACTIONS
         */

        public async Task<InfoTransactionResponse> CreateTransactionAsync(CreateTransactionRequest request, int userId)
        {
            bool justForRecord = request.JustForRecord;
            var transactionToCreate = _mapper.Map<Transaction>(request);

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!justForRecord) {
                        var account = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == transactionToCreate.AccountId) ?? throw new Exception("Conta não encontrada.");

                        if (transactionToCreate.Type == TransactionTypeEnum.EXPENSE)
                        {
                            /*if (account.Balance < transactionToCreate.Amount)
                            {
                                throw new Exception("Valor em conta é menor que o valor da transação.");
                            }*/

                            account.Balance -= transactionToCreate.Amount;
                        } else if (transactionToCreate.Type == TransactionTypeEnum.INCOME)
                        {
                            account.Balance += transactionToCreate.Amount;
                        }
                        

                        _appDbContext.Update(account);

                        await _appDbContext.SaveChangesAsync();
                    }

                    var createdTransaction = await _appDbContext.Transactions.AddAsync(transactionToCreate);

                    await _appDbContext.SaveChangesAsync();

                    transaction.Commit();


                    return _mapper.Map<InfoTransactionResponse>(createdTransaction.Entity);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

        }

        public async Task DeleteTransactionAsync(int expenseId, int userId)
        {
            var transactionToDelete = await _appDbContext.Transactions.FirstOrDefaultAsync(e => e.Id == expenseId) ?? throw new Exception("Transação não encontrada");
            bool justForRecord = transactionToDelete.JustForRecord;

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!justForRecord)
                    {
                        var account = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == transactionToDelete.AccountId) ?? throw new Exception("Conta não encontrada.");

                        account.Balance += transactionToDelete.Amount;

                        _appDbContext.Update(account);

                        await _appDbContext.SaveChangesAsync();
                    }

                    _appDbContext.Transactions.Remove(transactionToDelete);

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

        public async Task<InfoTransactionResponse> UpdateTransactionAsync(UpdateTransactionRequest request, int userId)
        {
            var transactionToUpdate = await _appDbContext.Transactions.Include(e => e.Account).FirstOrDefaultAsync(e => e.Id == request.Id) ?? throw new Exception("Conta não encontrada.");

            transactionToUpdate.UpdatedAt = DateTime.Now;

            if (transactionToUpdate.Account!.UserId != userId)
            {
                throw new Exception("Não autorizado: Transação não pertence a usuário.");
            }


            double amountDifToAccount = 0;
            bool justForRecord = transactionToUpdate.JustForRecord;

            if (request.Description is not null)
                transactionToUpdate.Description = request.Description;
            if (request.Destination is not null)
                transactionToUpdate.Destination = request.Destination;
            if (request.Observations is not null)
                transactionToUpdate.Observations = request.Observations;
            if (request.PurchaseDate is not null)
                transactionToUpdate.PurchaseDate = (DateTime)request.PurchaseDate;
            if (request.Amount is not null)
            {
                amountDifToAccount = transactionToUpdate.Amount - (double)request.Amount;

                if (transactionToUpdate.Type == TransactionTypeEnum.INCOME)
                {
                    amountDifToAccount *= -1;
                }

                transactionToUpdate.Amount = (double)request.Amount;
            }

            if (request.CategoryId is not null)
            {
                var category = await _appDbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId) 
                    ?? throw new Exception("Nova categória não encontrada.");

                if (category.BillType == BillTypeEnum.EXPENSE && transactionToUpdate.Type == TransactionTypeEnum.EXPENSE ||
                    category.BillType == BillTypeEnum.INCOME && transactionToUpdate.Type == TransactionTypeEnum.INCOME)
                {
                    transactionToUpdate.CategoryId = category.Id;
                } else
                {
                    throw new Exception("Nova categória é de um tipo diferente.");
                }

            }


            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!justForRecord && amountDifToAccount != 0) {
                    
                        var account = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == transactionToUpdate.AccountId) ?? throw new Exception("Conta não encontrada.");

                        account.Balance += amountDifToAccount;

                        _appDbContext.Update(account);

                        await _appDbContext.SaveChangesAsync();
                    }

                    var updatedTransaction = _appDbContext.Transactions.Update(transactionToUpdate);

                    await _appDbContext.SaveChangesAsync();

                    transaction.Commit();

                    return _mapper.Map<InfoTransactionResponse>(updatedTransaction.Entity);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            
        }


        /*
         * ========= TRANSFERENCE
         */

        public async Task<InfoTransferenceResponse> CreateTransferenceAsync(CreateTransferenceRequest request, int userId)
        {
            var transferenceToCreate = _mapper.Map<Transference>(request);

            var accountDestiny = await _appDbContext.Accounts.FirstAsync(x => x.Id == request.AccountDestinyId) ?? throw new Exception("Conta destino não encontrada.");

            var accountOrigin = await _appDbContext.Accounts.FirstAsync(x => x.Id == request.AccountOriginId) ?? throw new Exception("Conta origem não encontrada.");

            if (accountOrigin.Balance < request.Amount)
            {
                throw new Exception("Conta origem não possui saldo suficiente para essa transferência.");
            }

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    accountDestiny.Balance += request.Amount;
                    accountOrigin.Balance -= request.Amount;

                    _appDbContext.Update(accountOrigin);
                    _appDbContext.Update(accountDestiny);


                    var createdTransference = await _appDbContext.Transferences.AddAsync(transferenceToCreate);

                    await _appDbContext.SaveChangesAsync();

                    transaction.Commit();

                    return new InfoTransferenceResponse(createdTransference.Entity);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }



        /*
         * ========= GET TRANSACTIONS
         */

        public async Task<TransactionList> GetTransactionsAsync(int userId, DateTime startDate, DateTime endDate, int? accountId)
        {
            var transactions = await _appDbContext.Transactions.Include(i => i.Category)
                .Include(t => t.Account)
                .Where(t =>
                    t.Type != TransactionTypeEnum.CREDITEXPENSE &&
                    t.Account!.UserId == userId &&
                    t.PurchaseDate >= startDate &&
                    t.PurchaseDate <= endDate)
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();

            var invoices = await _appDbContext.Invoices
                .Include(i => i.Transactions)
                .Include(i => i.CreditCard)
                .Where(i => i.CreditCard.Account!.UserId == userId &&
                    i.ClosingDate >= startDate &&
                    i.ClosingDate <= endDate)
                .ToListAsync();

            if (accountId.HasValue)
            {

                var _ = await _appDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId) ??
                    throw new Exception("Conta não encontrada.");


                transactions = transactions.Where(t => t.AccountId == accountId).ToList();
                invoices = invoices.Where(t => t.CreditCard.AccountId == accountId).ToList();
            }

            return
                new TransactionList
                {
                    Transactions = _mapper.Map<List<InfoTransactionResponse>>(transactions),
                    Invoices = _mapper.Map<List<InfoInvoiceResponse>>(invoices)
                };
        }
        
    }
}