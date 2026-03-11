using AutoMapper;
using ControleCerto.DTOs.Common;
using ControleCerto.DTOs.Invoice;
using ControleCerto.DTOs.Transaction;
using ControleCerto.DTOs.TransferenceDTO;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.DTOs;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using ControleCerto.Validations;
using Microsoft.EntityFrameworkCore;
using System.Data;
using MassTransit.Initializers;

namespace ControleCerto.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IBalanceService _balanceService;
        
        public TransactionService(AppDbContext appDbContext, IMapper mapper, IBalanceService balanceService)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _balanceService = balanceService;
        }

        /*
         * ========= TRANSACTIONS
         */

        public async Task<Result<InfoTransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request, int userId)
        {
            var transactionToCreate = _mapper.Map<Transaction>(request);

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!request.JustForRecord) 
                    {
                        // 1. Buscar conta
                        var account = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == transactionToCreate.AccountId);
                        if (account is null)
                        {
                            return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
                        }

                        var balanceValidation = BusinessValidations.ValidateAccountBalance(account, transactionToCreate.Amount, transactionToCreate.Type);
                        if (balanceValidation.IsError) 
                        {
                            return balanceValidation.Error;
                        }

                        // 3. Atualizar saldo usando serviço centralizado (sem commit)
                        var balanceResult = _balanceService.UpdateAccountBalance(account, transactionToCreate.Amount, transactionToCreate.Type);
                        if (balanceResult.IsError) 
                        {
                            return balanceResult.Error;
                        }
                    }

                    // 4. Criar transação
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

            if (transactionToDelete.Type == TransactionTypeEnum.TRANSFERENCE) 
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
                // Usar validação centralizada para categoria
                var categoryValidation = await BusinessValidations.ValidateCategoryTypeAsync(request.CategoryId.Value, transactionToUpdate.Type, _appDbContext);
                if (categoryValidation.IsError)
                {
                    return categoryValidation.Error;
                }

                transactionToUpdate.CategoryId = request.CategoryId.Value;
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

            // 1. Buscar contas
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

            // 2. Validar saldo usando validação centralizada
            var balanceValidation = BusinessValidations.ValidateTransferAmount(accountOrigin, request.Amount);
            if (balanceValidation.IsError)
            {
                return balanceValidation.Error;
            }

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    // 3. Processar transferência usando serviço centralizado (sem commit)
                    var transferResult = _balanceService.ProcessTransferBalance(accountOrigin, accountDestiny, request.Amount);
                    if (transferResult.IsError)
                    {
                        return transferResult.Error;
                    }

                    // 4. Criar registro de transferência
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

        public async Task<Result<object>> GetTransactionsAsync(
            int userId, 
            DateTime startDate, 
            DateTime endDate, 
            string mode = "statement",
            int? accountId = null,
            long? cardId = null,
            long? categoryId = null,
            string? searchText = null,
            string? sort = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            // Validate mode
            if (!mode.Equals("invoice", StringComparison.OrdinalIgnoreCase) && 
                !mode.Equals("statement", StringComparison.OrdinalIgnoreCase))
            {
                return new AppError("Modo inválido. Use 'invoice' ou 'statement'.", ErrorTypeEnum.Validation);
            }

            // Validate pagination
            if (pageNumber < 1)
            {
                return new AppError("Parâmetro de paginação inválido. pageNumber deve ser >= 1.", ErrorTypeEnum.Validation);
            }

            mode = mode.ToLower();

            // === INVOICE MODE ===
            if (mode == "invoice")
            {
                return await GetTransactionsInvoiceMode(userId, startDate, accountId);
            }

            // === STATEMENT MODE ===
            return await GetTransactionsStatementMode(userId, startDate, endDate, accountId, cardId, categoryId, searchText, sort, pageNumber, pageSize);
        }

        /// <summary>
        /// Invoice mode: Returns monthly invoices with their transactions.
        /// Only uses month/year from startDate, ignores day.
        /// </summary>
        private async Task<Result<object>> GetTransactionsInvoiceMode(int userId, DateTime startDate, int? accountId)
        {
            var startDateInvoiceFilter = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var transactions = await _appDbContext.Transactions
                .Include(i => i.Category)
                .Include(t => t.Account)
                .Where(t =>
                    t.Type != TransactionTypeEnum.CREDITEXPENSE &&
                    t.Account!.UserId == userId &&
                    t.PurchaseDate >= startDateInvoiceFilter &&
                    t.PurchaseDate < startDateInvoiceFilter.AddMonths(1))
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();

            var invoicePayments = await _appDbContext.InvoicePayments
                .Include(ip => ip.Account)
                .Where(ip =>
                    ip.Account!.UserId == userId &&
                    ip.PaymentDate >= startDateInvoiceFilter &&
                    ip.PaymentDate < startDateInvoiceFilter.AddMonths(1))
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

            var allTransactions = _mapper.Map<List<InfoTransactionResponse>>(transactions);
            allTransactions.AddRange(invoicePayments.Select(ip => new InfoTransactionResponse(ip)));

            return new InvoiceListResponse
            {
                Transactions = allTransactions,
                Invoices = _mapper.Map<List<InfoInvoiceResponse>>(invoices)
            };
        }

        /// <summary>
        /// Statement mode: Returns all transactions as a detailed ledger with advanced filtering and sorting.
        /// </summary>
        private async Task<Result<object>> GetTransactionsStatementMode(
            int userId,
            DateTime startDate,
            DateTime endDate,
            int? accountId,
            long? cardId,
            long? categoryId,
            string? searchText,
            string? sort,
            int pageNumber,
            int pageSize)
        {
            // Ensure endDate includes the entire last day
            var adjustedEndDate = endDate.Date.AddDays(1).AddTicks(-1);

            // Build base queries
            var transactions = await _appDbContext.Transactions
                .Include(i => i.Category)
                .Include(t => t.Account)
                .Where(t =>
                    t.Type != TransactionTypeEnum.CREDITEXPENSE &&
                    t.Account!.UserId == userId &&
                    t.PurchaseDate >= startDate &&
                    t.PurchaseDate <= adjustedEndDate)
                .ToListAsync();

            var invoicePayments = await _appDbContext.InvoicePayments
                .Include(ip => ip.Account)
                .Where(ip =>
                    ip.Account!.UserId == userId &&
                    ip.PaymentDate >= startDate &&
                    ip.PaymentDate <= adjustedEndDate)
                .ToListAsync();

            var creditPurchases = await _appDbContext.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Include(t => t.CreditPurchase)
                .Where(t =>
                    t.Type == TransactionTypeEnum.CREDITEXPENSE &&
                    t.Account!.UserId == userId &&
                    t.PurchaseDate >= startDate &&
                    t.PurchaseDate <= adjustedEndDate)
                .ToListAsync();

            // Apply resource filters
            if (accountId.HasValue)
            {
                var account = await _appDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);
                if (account is null)
                {
                    return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
                }

                transactions = transactions.Where(t => t.AccountId == accountId).ToList();
                invoicePayments = invoicePayments.Where(t => t.AccountId == accountId).ToList();
                creditPurchases = creditPurchases.Where(t => t.AccountId == accountId).ToList();
            }

            if (cardId.HasValue)
            {
                creditPurchases = creditPurchases
                    .Where(t => t.CreditPurchase?.CreditCardId == cardId)
                    .ToList();
            }

            if (categoryId.HasValue)
            {
                transactions = transactions.Where(t => t.CategoryId == categoryId).ToList();
                creditPurchases = creditPurchases.Where(t => t.CategoryId == categoryId).ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var searchLower = searchText.ToLower();
                transactions = transactions
                    .Where(t => t.Description.ToLower().Contains(searchLower) || 
                               (t.Destination?.ToLower().Contains(searchLower) ?? false) ||
                               (t.Observations?.ToLower().Contains(searchLower) ?? false))
                    .ToList();

                invoicePayments = invoicePayments
                    .Where(ip => ip.Description.ToLower().Contains(searchLower))
                    .ToList();

                creditPurchases = creditPurchases
                    .Where(t => t.Description.ToLower().Contains(searchLower) ||
                               (t.Destination?.ToLower().Contains(searchLower) ?? false))
                    .ToList();
            }

            // Merge all transaction types into unified view
            var allTransactions = new List<InfoTransactionResponse>();
            allTransactions.AddRange(_mapper.Map<List<InfoTransactionResponse>>(transactions));
            allTransactions.AddRange(invoicePayments.Select(ip => new InfoTransactionResponse(ip)));
            allTransactions.AddRange(_mapper.Map<List<InfoTransactionResponse>>(creditPurchases));

            // Apply sorting
            allTransactions = ApplySorting(allTransactions, sort);

            // Calculate summary
            var summary = new StatementSummary
            {
                TotalItems = allTransactions.Count,
                TotalIncome = allTransactions
                    .Where(t => t.Type == TransactionTypeEnum.INCOME)
                    .Sum(t => t.Amount),
                TotalExpense = allTransactions
                    .Where(t => t.Type == TransactionTypeEnum.EXPENSE)
                    .Sum(t => t.Amount),
                TotalCreditCharges = allTransactions
                    .Where(t => t.Type == TransactionTypeEnum.CREDITEXPENSE)
                    .Sum(t => t.Amount),
                TotalInvoicePayments = allTransactions
                    .Where(t => t.Type == TransactionTypeEnum.INVOICEPAYMENT)
                    .Sum(t => t.Amount),
            };

            summary.NetBalance = summary.TotalIncome - summary.TotalExpense - summary.TotalCreditCharges;

            // Apply pagination AFTER calculating summary over complete filtered set
            int totalItems = allTransactions.Count;
            var paginatedTransactions = allTransactions
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagination = new PaginationMetadata(pageNumber, pageSize, totalItems);

            return new StatementResponsePaginated
            {
                StartDate = startDate,
                EndDate = adjustedEndDate,
                Transactions = new PaginatedResponse<InfoTransactionResponse>
                {
                    Data = paginatedTransactions,
                    Pagination = pagination
                },
                Summary = summary
            };
        }

        /// <summary>
        /// Applies dynamic sorting to transaction list.
        /// Sort format: "field asc|desc,field asc|desc"
        /// Valid fields: date, amount, account, category
        /// </summary>
        private List<InfoTransactionResponse> ApplySorting(List<InfoTransactionResponse> transactions, string? sort)
        {
            if (string.IsNullOrWhiteSpace(sort))
            {
                // Default: most recent first
                return transactions.OrderByDescending(t => t.PurchaseDate).ToList();
            }

            var sortParts = sort.Split(',');
            IOrderedEnumerable<InfoTransactionResponse> orderedQuery = null;

            foreach (var part in sortParts)
            {
                var trimmedPart = part.Trim();
                var components = trimmedPart.Split(' ');
                
                if (components.Length != 2)
                    continue;

                var field = components[0].ToLower();
                var direction = components[1].ToLower();
                var isAscending = direction == "asc";

                if (orderedQuery == null)
                {
                    orderedQuery = field switch
                    {
                        "date" => isAscending 
                            ? transactions.OrderBy(t => t.PurchaseDate)
                            : transactions.OrderByDescending(t => t.PurchaseDate),
                        "amount" => isAscending
                            ? transactions.OrderBy(t => t.Amount)
                            : transactions.OrderByDescending(t => t.Amount),
                        "account" => isAscending
                            ? transactions.OrderBy(t => t.Account?.Description ?? "")
                            : transactions.OrderByDescending(t => t.Account?.Description ?? ""),
                        "category" => isAscending
                            ? transactions.OrderBy(t => t.Category?.Name ?? "")
                            : transactions.OrderByDescending(t => t.Category?.Name ?? ""),
                        _ => transactions.OrderByDescending(t => t.PurchaseDate)
                    };
                }
                else
                {
                    orderedQuery = field switch
                    {
                        "date" => isAscending
                            ? orderedQuery.ThenBy(t => t.PurchaseDate)
                            : orderedQuery.ThenByDescending(t => t.PurchaseDate),
                        "amount" => isAscending
                            ? orderedQuery.ThenBy(t => t.Amount)
                            : orderedQuery.ThenByDescending(t => t.Amount),
                        "account" => isAscending
                            ? orderedQuery.ThenBy(t => t.Account?.Description ?? "")
                            : orderedQuery.ThenByDescending(t => t.Account?.Description ?? ""),
                        "category" => isAscending
                            ? orderedQuery.ThenBy(t => t.Category?.Name ?? "")
                            : orderedQuery.ThenByDescending(t => t.Category?.Name ?? ""),
                        _ => orderedQuery
                    };
                }
            }

            return orderedQuery?.ToList() ?? transactions.OrderByDescending(t => t.PurchaseDate).ToList();
        }
        
    }
}