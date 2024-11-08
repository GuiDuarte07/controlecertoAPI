using AutoMapper;
using ControleCerto.DTOs.Invoice;
using ControleCerto.DTOs.Transaction;
using ControleCerto.DTOs.TransferenceDTO;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.DTOs;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
using MassTransit.Initializers;

namespace ControleCerto.Services
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

        public async Task<Result<InfoTransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request, int userId)
        {
            bool justForRecord = request.JustForRecord;
            var transactionToCreate = _mapper.Map<Transaction>(request);

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!justForRecord) {
                        var account = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == transactionToCreate.AccountId);

                        if (account is null)
                        {
                            return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
                        }

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

        public async Task<Result<bool>> DeleteTransactionAsync(int expenseId, int userId)
        {
            var transactionToDelete = await _appDbContext.Transactions.FirstOrDefaultAsync(e => e.Id == expenseId);

            if (transactionToDelete is null)
            {
                return new AppError("Transação não encontrada.", ErrorTypeEnum.NotFound);
            }

            bool justForRecord = transactionToDelete.JustForRecord;

            if (transactionToDelete.Type == TransactionTypeEnum.CREDITEXPENSE || transactionToDelete.Type == TransactionTypeEnum.TRANSFERENCE) 
            {
                return new AppError("Exclusão de transação não implementada para esse tipo ainda.", ErrorTypeEnum.NotImplemented);
            }

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!justForRecord)
                    {
                        var account = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == transactionToDelete.AccountId);

                        if (account is null)
                        {
                            return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
                        }

                        switch (transactionToDelete.Type)
                        {
                            case TransactionTypeEnum.INCOME:
                                account.Balance -= transactionToDelete.Amount;
                                break;
                            case TransactionTypeEnum.EXPENSE:
                                account.Balance += transactionToDelete.Amount;
                                break;
                            default:
                                return new AppError("Tipo de transação não suportado.", ErrorTypeEnum.BusinessRule);
                        }

                        _appDbContext.Update(account);

                        await _appDbContext.SaveChangesAsync();
                    }

                    _appDbContext.Transactions.Remove(transactionToDelete);

                    await _appDbContext.SaveChangesAsync();

                    transaction.Commit();

                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<Result<InfoTransactionResponse>> UpdateTransactionAsync(UpdateTransactionRequest request, int userId)
        {
            var transactionToUpdate = await _appDbContext.Transactions.Include(e => e.Account).FirstOrDefaultAsync(e => e.Id == request.Id);

            if (transactionToUpdate is null || transactionToUpdate.Account!.UserId != userId)
            {
                return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
            }

            transactionToUpdate.UpdatedAt = DateTime.UtcNow;

            var totalAmount = request.Amount ?? transactionToUpdate.Amount;
            double amountDifToAccount = 0;

            if (request.Description is not null)
                transactionToUpdate.Description = request.Description;
            if (request.Destination is not null)
                transactionToUpdate.Destination = request.Destination;
            if (request.Observations is not null)
                transactionToUpdate.Observations = request.Observations;
            if (request.PurchaseDate is not null)
                transactionToUpdate.PurchaseDate = (DateTime)request.PurchaseDate;


            var justForRecord = request.JustForRecord;
            
            // Se null JustForRecord não foi atualizado, se true, JustForRecord foi atualizado de false para true, se false, foi atualizado de false para true
            bool? recalculateBalanceFromJustForRecord = null;
            if (request.JustForRecord is not null)
            {
                if (request.JustForRecord != transactionToUpdate.JustForRecord)
                {
                    recalculateBalanceFromJustForRecord = request.JustForRecord;
                }
                
                transactionToUpdate.JustForRecord = (bool)request.JustForRecord;
            }

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
                var category = await _appDbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId);

                if (category is null)
                {
                    return new AppError("Nova categória não encontrada.", ErrorTypeEnum.Validation);
                }

                if (category.BillType == BillTypeEnum.EXPENSE && transactionToUpdate.Type == TransactionTypeEnum.EXPENSE ||
                    category.BillType == BillTypeEnum.INCOME && transactionToUpdate.Type == TransactionTypeEnum.INCOME)
                {
                    transactionToUpdate.CategoryId = category.Id;
                } else
                {
                    return new AppError("Nova categória é de um tipo diferente.", ErrorTypeEnum.BusinessRule);
                }

            }


            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (recalculateBalanceFromJustForRecord is not null || amountDifToAccount != 0) {
                    
                        var account = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == transactionToUpdate.AccountId);

                        if (account is null)
                        {
                            return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
                        }

                        if (
                            amountDifToAccount != 0 && 
                            recalculateBalanceFromJustForRecord is null && 
                            transactionToUpdate.JustForRecord == false
                            )
                        {
                            account.Balance += amountDifToAccount;
                        }
                        
                        switch (recalculateBalanceFromJustForRecord)
                        {
                            case false:
                                if (transactionToUpdate.Type == TransactionTypeEnum.INCOME)
                                {
                                    account.Balance += totalAmount;
                                }
                                else
                                {
                                    account.Balance -= totalAmount;
                                }
                                break;
                            case true:
                                if (transactionToUpdate.Type == TransactionTypeEnum.INCOME)
                                {
                                    account.Balance -= totalAmount;
                                }
                                else
                                {
                                    account.Balance += totalAmount;
                                }
                                break;
                        }

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

        public async Task<Result<InfoTransferenceResponse>> CreateTransferenceAsync(CreateTransferenceRequest request, int userId)
        {
            var transferenceToCreate = _mapper.Map<Transference>(request);

            var accountDestiny = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == request.AccountDestinyId);

            if (accountDestiny is null)
            {
                return new AppError("Conta destino não encontrada.", ErrorTypeEnum.NotFound);
            }

            var accountOrigin = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == request.AccountOriginId);

            if (accountOrigin is null)
            {
                return new AppError("Conta origem não encontrada.", ErrorTypeEnum.NotFound);
            }

            if (accountOrigin.Balance < Math.Round(request.Amount, 2))
            {
                return new AppError("Conta origem não possui saldo suficiente para essa transferência.", ErrorTypeEnum.BusinessRule);
            }

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    accountDestiny.Balance = Math.Round(accountDestiny.Balance, 2) + Math.Round(request.Amount, 2);
                    accountOrigin.Balance = Math.Round(accountOrigin.Balance, 2) - Math.Round(request.Amount, 2);

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

        public async Task<Result<TransactionList>> GetTransactionsAsync(int userId, DateTime startDate, DateTime endDate, bool? seeInvoices, int? accountId)
        {
            var startDateInvoiceFilter = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            if (seeInvoices is null || seeInvoices is true)
            {
                var transactions = await _appDbContext.Transactions
                .Include(i => i.Category)
                .Include(t => t.Account)
                .Where(t =>
                    t.Type != TransactionTypeEnum.CREDITEXPENSE &&
                    t.Account!.UserId == userId &&
                    t.PurchaseDate >= startDate &&
                    t.PurchaseDate <= endDate)
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();

                var invoicePayments = await _appDbContext.InvoicePayments
                    .Include(ip => ip.Account)
                    .Where(ip =>
                        ip.Account!.UserId == userId &&
                        ip.PaymentDate >= startDate &&
                        ip.PaymentDate <= endDate)
                    .OrderByDescending(t => t.PaymentDate)
                    .ToListAsync();

                var invoices = await _appDbContext.Invoices
                    .Include(i => i.Transactions)
                        .ThenInclude(t => t.Category)
                    .Include(i => i.Transactions)
                        .ThenInclude(t => t.CreditPurchase)
                   .Include(i => i.Transactions)
                        .ThenInclude(t => t.Account)
                    .Include(i => i.CreditCard)
                    .Where(i => i.CreditCard.Account!.UserId == userId &&
                        i.InvoiceDate.Equals(startDateInvoiceFilter))
                    .ToListAsync();

                if (accountId.HasValue)
                {

                    var account = await _appDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

                    if (account is null)
                    {
                        return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
                    }


                    transactions = transactions.Where(t => t.AccountId == accountId).ToList();
                    invoicePayments = invoicePayments.Where(t => t.AccountId == accountId).ToList();
                    invoices = invoices.Where(t => t.CreditCard.AccountId == accountId).ToList();
                }

                var allTransactions =
                    _mapper.Map<List<InfoTransactionResponse>>(transactions);

                allTransactions.AddRange(invoicePayments
                    .Select(ip => new InfoTransactionResponse(ip)));

                return
                    new TransactionList
                    {
                        Transactions = allTransactions,
                        Invoices = _mapper.Map<List<InfoInvoiceResponse>>(invoices)
                    };
            } else
            {
                var transactions = await _appDbContext.Transactions
                .Include(i => i.Category)
                .Include(t => t.Account)
                .Where(t =>
                    t.Account!.UserId == userId &&
                    t.PurchaseDate >= startDate &&
                    t.PurchaseDate <= endDate)
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();

                if (accountId.HasValue)
                {

                    var account = await _appDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

                    if (account is null)
                    {
                        return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
                    }


                    transactions = transactions.Where(t => t.AccountId == accountId).ToList();
                }

                var allTransactions =
                    _mapper.Map<List<InfoTransactionResponse>>(transactions);

                return
                    new TransactionList
                    {
                        Transactions = allTransactions,
                        Invoices = []
                    };
            }
        }
        
    }
}