using AutoMapper;
using ControleCerto.DTOs.CreditCard;
using ControleCerto.DTOs.CreditPurchase;
using ControleCerto.DTOs.Invoice;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.DTOs;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace ControleCerto.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        public CreditCardService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<Result<InfoCreditCardResponse>> CreateCreditCardAsync(CreateCreditCardRequest request, int userId)
        {
            if (request.CloseDay > request.DueDay)
            {
                return new AppError("O sistema não suporta data de vencimento maior que a data de fechamento.", ErrorTypeEnum.BusinessRule);
            }
            
            var creditCardToCreate = _mapper.Map<CreditCard>(request);
            var account = await _appDbContext.Accounts.Include(a => a.CreditCard).FirstOrDefaultAsync(cd => cd.Id == creditCardToCreate.AccountId);

            if (account is null || account.UserId != userId)
            {
                return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
            }

            if (account.CreditCard is not null)
            {
                return new AppError("Conta já possui um cartão.", ErrorTypeEnum.Validation);
            }

            var createdCreditCard = await _appDbContext.CreditCards.AddAsync(creditCardToCreate);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoCreditCardResponse>(createdCreditCard.Entity);
        }

        public async Task<Result<InfoCreditCardResponse>> UpdateCreditCardAsync(UpdateCreditCardRequest request, int userId)
        {
            var creditCardToUpdate = await _appDbContext
                .CreditCards.Include(cd => cd.Account)
                .FirstOrDefaultAsync(cd => cd.Id == request.Id);

            if (creditCardToUpdate is null || creditCardToUpdate.Account.UserId != userId)
            {
                return new AppError("Cartão de crédito não encontrado.", ErrorTypeEnum.NotFound);
            }
            
            creditCardToUpdate.UpdatedAt = DateTime.UtcNow;

            if (request.Description is not null)
                creditCardToUpdate.Description = request.Description;
            if (request.TotalLimit is not null && request.TotalLimit >= 0)
                creditCardToUpdate.TotalLimit = (double)request.TotalLimit;

            /*
             * Será feito posteriormente a lógica em mudar o UsedLimit, DueDate e CloseDate
             */

            /*
                if (request.CloseDay is not null)
                    creditCardToUpdate.CloseDay = (int)request.CloseDay;
                if (request.DueDay is not null)
                    creditCardToUpdate.DueDay = (int)request.DueDay;
                
                if (request.UsedLimit is not null && request.UsedLimit >= 0)
                    creditCardToUpdate.UsedLimit = (double)request.UsedLimit;
            */

            var updatedCreditCard = _appDbContext.CreditCards.Update(creditCardToUpdate);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoCreditCardResponse>(updatedCreditCard.Entity);
        }
        
        public async Task<Result<InfoCreditPurchaseResponse>> CreateCreditPurchaseAsync(CreateCreditPurchaseRequest request, int userId)
        {
            var creditPurchaseToCreate = _mapper.Map<CreditPurchase>(request);

            var creditCard = await _appDbContext.CreditCards.Include(cc => cc.Account).FirstOrDefaultAsync(cc => cc.Id == request.CreditCardId && cc.Account.UserId == userId);

            if (creditCard is null)
            {
                return new AppError("Cartão de crédito não encontrada.", ErrorTypeEnum.NotFound);
            }

            if (creditCard.TotalLimit - creditCard.UsedLimit < request.TotalAmount)
                return new AppError("Limite inferior ao valor da compra.", ErrorTypeEnum.Validation);

            var category = await _appDbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == userId);

            if (category is null)
            {
                return new AppError("Categoria informada não encontrada.", ErrorTypeEnum.NotFound);
            }

            if (category.BillType != BillTypeEnum.EXPENSE)
            {
                return new AppError("Essa categoria não é de gastos.", ErrorTypeEnum.Validation);
            }

            /*
             * ===== VALIDAR O FUNCIONAMENTO PARA REGISTRO DE COMPRAS ANTIGAS ======
             */

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    double TotalAmount = request.TotalAmount;
                    int totalInstallment = request.TotalInstallment;
                    int installmentsPaid = request.InstallmentsPaid;
                    double installmentAmount = TotalAmount / (request.TotalInstallment - installmentsPaid);

                    /*
                     * Detalhes sobre implementação:
                     * - Se a data de fechamento da fatura é dia 20 p.e., as compras do dia 20 a 27 entram na fatura do próximo mês
                     * - Se a compra foi feito antes do fechamento, ex: dia 19, a primeira parcela da compra já entra na fatura do mês
                     * * Por enquanto estamos supondo que o dia de vencimento possa ser entre dia 8 e 28 ---------
                     *   (ou seja, não corre o risco de a fatura do mês acaber sendo paga no começo do outro mês).
                     */

                    var createdCreditPurchase = await _appDbContext.CreditPurchases.AddAsync(creditPurchaseToCreate);
                    await _appDbContext.SaveChangesAsync();

                    // Encerramento e Fechamento da Fatura
                    int invoiceCloseDay = creditCard.CloseDay; 
                    var invoiceDueDay = creditCard.DueDay; 
                    
                    var purchaseDay = createdCreditPurchase.Entity.PurchaseDate.Day; 

                    var actualMonthInvoice = new DateTime(createdCreditPurchase.Entity.PurchaseDate.Year,
                        createdCreditPurchase.Entity.PurchaseDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                    // Pegar o dia de encerramento e fechamento da fatura do mês da compra
                    var purchaseMonthInvoice = await _appDbContext.Invoices.FirstOrDefaultAsync(i => i.InvoiceDate == actualMonthInvoice);

                    if (purchaseMonthInvoice is not null)
                    {
                        invoiceCloseDay = purchaseMonthInvoice.ClosingDate.Day;
                        invoiceDueDay = purchaseMonthInvoice.DueDate.Day;
                    }

                    var isInClosingDate = purchaseDay >= invoiceCloseDay && purchaseDay <= invoiceDueDay;
                    var isAfterDueDate = purchaseDay > invoiceDueDay;

                    /*if (isInClosingDate == false && isAfterDueDate == false)
                    {
                        actualMonthInvoice = actualMonthInvoice.AddMonths(-1);
                    }*/

                    // Se essa fatura já está na sua data de fechamento, entra pra próxima.
                    if (isInClosingDate || isAfterDueDate)
                    {
                        actualMonthInvoice = actualMonthInvoice.AddMonths(1);
                    }

                    ICollection<Transaction> transactions = [];

                    for (var i = 0; i < totalInstallment - installmentsPaid; i++)
                    {
                        var monthInvoiceDate = actualMonthInvoice.AddMonths(i);

                        var monthInvoice = await _appDbContext.Invoices.FirstOrDefaultAsync
                        (
                            invoice => invoice.CreditCard.AccountId.Equals(creditCard.AccountId) && invoice.InvoiceDate.Equals(monthInvoiceDate)
                        );


                        if(monthInvoice is null)
                        {
                            var monthClosingInvoiceDate = new DateTime(monthInvoiceDate.Year, monthInvoiceDate.Month, creditCard.CloseDay, 0, 0, 0, DateTimeKind.Utc);
                            var monthDueInvoiceDate = new DateTime(monthInvoiceDate.Year, monthInvoiceDate.Month, creditCard.DueDay, 0, 0, 0, DateTimeKind.Utc);

                            if(creditCard.SkipWeekend)
                            {
                                if (monthClosingInvoiceDate.DayOfWeek == DayOfWeek.Saturday)
                                {
                                    monthClosingInvoiceDate = monthClosingInvoiceDate.AddDays(2);
                                    monthDueInvoiceDate = monthDueInvoiceDate.AddDays(2);
                                }
                                else if (monthClosingInvoiceDate.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    monthClosingInvoiceDate = monthClosingInvoiceDate.AddDays(1);
                                    monthDueInvoiceDate = monthDueInvoiceDate.AddDays(1);
                                }
                            }

                            var newInvoice = new Invoice
                            {
                                InvoiceDate = monthInvoiceDate,
                                ClosingDate = monthClosingInvoiceDate,
                                DueDate = monthDueInvoiceDate,
                                CreditCardId = creditCard.Id,
                            };

                            var invoiceCreated = await _appDbContext.Invoices.AddAsync(newInvoice);
                            await _appDbContext.SaveChangesAsync();

                            monthInvoice = invoiceCreated.Entity;
                        }
                        

                        var newTransaction = new Transaction
                        {
                            Type = TransactionTypeEnum.CREDITEXPENSE,
                            CreditPurchaseId = createdCreditPurchase.Entity.Id,
                            AccountId = creditCard.AccountId,
                            CategoryId = request.CategoryId,
                            Amount = installmentAmount,
                            InstallmentNumber = i + 1 + installmentsPaid,
                            InvoiceId = monthInvoice.Id,
                            Description = request.Description + 
                                (totalInstallment > 1 ? $" {i + 1 + installmentsPaid}/{(totalInstallment - installmentsPaid)}" : ""),
                            Destination = request.Destination,
                            PurchaseDate = request.PurchaseDate ?? DateTime.UtcNow,
                        };


                        transactions.Add(newTransaction);
                        monthInvoice.TotalAmount += installmentAmount;
                        if(monthInvoice.IsPaid)
                        {
                            monthInvoice.IsPaid = false;
                        }
                        _appDbContext.Invoices.Update(monthInvoice);
                    }


                    creditCard.UsedLimit += request.TotalAmount;
                    await _appDbContext.Transactions.AddRangeAsync(transactions);
                    _appDbContext.CreditCards.Update(creditCard);

                    await _appDbContext.SaveChangesAsync();

                    transaction.Commit();

                    return _mapper.Map<InfoCreditPurchaseResponse>(createdCreditPurchase.Entity);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<Result<IEnumerable<InfoInvoiceResponse>>> GetInvoicesByDateAsync(int userId, DateTime startDate, DateTime endDate, long? creditCardId)
        {
            if (creditCardId.HasValue)
            {
                var creditCard = await _appDbContext.CreditCards.FirstOrDefaultAsync(cc => cc.Id == creditCardId && cc.Account.UserId == userId);

                if (creditCard is null)
                {
                    return new AppError("Cartão de crédito não encontrado.", ErrorTypeEnum.NotFound);
                }

            }

            var invoices = await _appDbContext.Invoices
                .Include(i => i.CreditCard)
                .Where(i =>
                i.CreditCard.Account.UserId == userId &&
                i.InvoiceDate >= startDate &&
                i.InvoiceDate <= endDate && 
                i.CreditCard.Id == (creditCardId ?? i.CreditCard.Id)
            )
            .OrderByDescending(a => a.InvoiceDate)
            .ToListAsync();            

            return _mapper.Map<List<InfoInvoiceResponse>>(invoices);
        }

        public async Task<Result<InvoicePageResponse>> GetInvoicesByIdAsync(long invoiceId, int userId)
        {
            var invoice = await _appDbContext.Invoices
                .Include(i => i.CreditCard)
                .Include(i => i.Transactions)
                    .ThenInclude(t => t.Account)
                .Include(i => i.Transactions)
                    .ThenInclude(t => t.Category)
                .Include(i => i.InvoicePayments)
                    .ThenInclude(ip => ip.Account)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.CreditCard.Account.UserId == userId);

            if(invoice is null)
            {
                return new AppError("Fatura não encontrada.", ErrorTypeEnum.NotFound);
            }
                    

            var nextInvoiceMonth = invoice.InvoiceDate.AddMonths(1);
            var prevInvoiceMonth = invoice.InvoiceDate.AddMonths(-1);

            var nextInvoice = await _appDbContext.Invoices.FirstOrDefaultAsync(i => i.CreditCardId == invoice.CreditCardId && i.InvoiceDate == nextInvoiceMonth);

            var prevInvoice = await _appDbContext.Invoices.FirstOrDefaultAsync(i => i.CreditCardId == invoice.CreditCardId && i.InvoiceDate == prevInvoiceMonth);

            return new InvoicePageResponse(_mapper.Map<InfoInvoiceResponse>(invoice), nextInvoice?.Id, prevInvoice?.Id);
        }

        public async Task<Result<InfoInvoicePaymentResponse>> PayInvoiceAsync(CreteInvoicePaymentRequest invoicePaymentRequest, int userId)
        {
            var invoicePayment = _mapper.Map<InvoicePayment>(invoicePaymentRequest);

            var invoice = await _appDbContext.Invoices.Include(i => i.CreditCard).FirstOrDefaultAsync(
                i => i.Id == invoicePayment.InvoiceId &&
                i.CreditCard.Account.UserId == userId
            );

            if (invoice is null)
            {
                return new AppError("Fatura não encontrada.", ErrorTypeEnum.NotFound);
            }

            var account = await _appDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == invoicePayment.AccountId && a.UserId == userId);

            if (account is null)
            {
                return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
            }

            if (invoice.IsPaid)
            {
                return new AppError("Essa fatura já está paga.", ErrorTypeEnum.Validation);
            }

            var amountRemainder = Math.Round(invoice.TotalAmount - invoice.TotalPaid, 2);

            if (invoicePayment.AmountPaid > amountRemainder)
            {
                return new AppError(
                    $"Quantidade a ser paga (R$ {invoicePayment.AmountPaid}) é maior que o valor da fatura restante (R$ {amountRemainder}).", 
                    ErrorTypeEnum.Validation
                );
            }

            invoice.TotalPaid =  Math.Round(invoice.TotalPaid + invoicePayment.AmountPaid, 2);
            invoice.CreditCard.UsedLimit -= invoicePayment.AmountPaid;

            const double lowerDiff = 1e-4;
            
            if (Math.Abs(invoice.TotalPaid - invoice.TotalAmount) < lowerDiff)
            {
                invoice.IsPaid = true;
            }

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!invoicePaymentRequest.JustForRecord)
                    {
                        account.Balance = Math.Round(
                            account.Balance - invoicePayment.AmountPaid, 2);
                        _appDbContext.Update(account);
                        await _appDbContext.SaveChangesAsync();

                    }
                    var createdInvoicePayment = await _appDbContext.InvoicePayments.AddAsync(invoicePayment);
                    _appDbContext.Update(invoice);
                    

                    await _appDbContext.SaveChangesAsync();

                    transaction.Commit();
                    return _mapper.Map<InfoInvoicePaymentResponse>(createdInvoicePayment.Entity);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            
        }

        public async Task<Result<bool>> DeleteInvoicePaymentAsync(long invoicePaymentId, int userId)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                var invoicePaymentToDelete =
                    await _appDbContext.InvoicePayments
                        .Include(ip => ip.Account)
                            .ThenInclude(a => a.CreditCard)
                        .Include(ip => ip.Invoice)
                        .FirstOrDefaultAsync(ip =>
                            ip.Id == invoicePaymentId);

                if (invoicePaymentToDelete is null)
                    return new AppError("Pagamento de fatura não encontrado.",
                        ErrorTypeEnum.NotFound);

                var account = invoicePaymentToDelete.Account;
                account.Balance = Math.Round(
                    invoicePaymentToDelete.Account.Balance +
                    invoicePaymentToDelete.AmountPaid, 2);

                var creditCard = account.CreditCard!;

                if (creditCard is null)
                {
                    return new AppError("Cartão de crédito não encontrado.",
                        ErrorTypeEnum.NotFound);
                }

                creditCard.UsedLimit = Math.Round(
                    creditCard.UsedLimit +
                    invoicePaymentToDelete.AmountPaid, 2);

                _appDbContext.Update(account);

                var invoice = invoicePaymentToDelete.Invoice;
                invoice.IsPaid = false;
                invoice.TotalPaid =
                    Math.Round(
                        invoice.TotalPaid - invoicePaymentToDelete.AmountPaid, 2);
            
                _appDbContext.Update(invoice);

                _appDbContext.Remove(invoicePaymentToDelete);

                await _appDbContext.SaveChangesAsync();
                
                await transaction.CommitAsync();

                return true;
            } catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new AppError("Ocorreu um erro ao deletar o pagamento da fatura.", ErrorTypeEnum.InternalError);
            }
        }

        public async Task<Result<bool>> DeleteCreditPurchaseAsync(long purchaseId, int userId)
        {
            var creditPurchaseToDelete = await _appDbContext.CreditPurchases.Include(cp => cp.Transactions).FirstOrDefaultAsync(cp => cp.Id == purchaseId && cp.CreditCard.Account.UserId == userId);

            if (creditPurchaseToDelete is null)
            {
                return new AppError("Compra cartão não encontrada.", ErrorTypeEnum.NotFound);
            }

            var creditCard = await _appDbContext.CreditCards.Include(cc => cc.Account).FirstOrDefaultAsync(cc => cc.Id == creditPurchaseToDelete.CreditCardId && cc.Account.UserId == userId);

            if (creditCard is null)
            {
                return new AppError("Cartão de Crédito não encontrado.", ErrorTypeEnum.NotFound);
            }

            var creditExpenses = creditPurchaseToDelete.Transactions;

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var expense in creditExpenses)
                    {
                        var invoice = await _appDbContext.Invoices.FirstOrDefaultAsync(cp => cp.Id == expense.InvoiceId);
                        
                        if (invoice is null)
                        {
                            return new AppError("Fatura não encontrada.", ErrorTypeEnum.NotFound);
                        }
                        
                        if (invoice.IsPaid)
                        {
                            return new AppError("Não é mais possível excluir esse registro pois a fatura já foi paga.", ErrorTypeEnum.BusinessRule);
                        }

                        if (Math.Round(invoice.TotalAmount - invoice.TotalPaid, 2) < creditPurchaseToDelete.TotalAmount)
                        {
                            return new AppError(
                                $"Essa fatura tem um valor de ${(invoice.TotalAmount - invoice.TotalPaid):F2} a ser pago ainda e essa transação a ser deletada " +
                                $"tem um valor de ${creditPurchaseToDelete.TotalAmount}, " +
                                $"não será possível deletar esse registro pois ao fazer isso o valor total " +
                                $"da fatura vai ser maior que o total que está pago. Para excluir esse registro " +
                                $"será necessário excluir o pagamento dessa fatura.",
                                ErrorTypeEnum.BusinessRule);
                        }

                        invoice.TotalAmount = Math.Round(invoice.TotalAmount - expense.Amount, 2);

                        if ((Math.Round(invoice.TotalAmount - invoice.TotalPaid, 2)).Equals(0.00d))
                        {
                            invoice.IsPaid = true;
                        }

                        _appDbContext.Update(invoice);
                    }

                    creditCard.UsedLimit -= creditPurchaseToDelete.TotalAmount;

                    _appDbContext.Transactions.RemoveRange(creditExpenses);
                    _appDbContext.CreditPurchases.Remove(creditPurchaseToDelete);

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

        public async Task<Result<InfoTransactionResponse[]>> GetCreditExpensesFromInvoice(int invoiceId, int userId)
        {
            var creditExpenses = await _appDbContext.Transactions.Where(ce =>ce.Type == TransactionTypeEnum.CREDITEXPENSE && ce.InvoiceId == invoiceId && ce.Account.UserId == userId).ToListAsync();

            return _mapper.Map<InfoTransactionResponse[]>(creditExpenses);
        }

        public async Task<Result<InfoCreditPurchaseResponse>> UpdateCreditPurchaseAsync(UpdateCreditPurchaseRequest request, int userId)
        {
            var creditPurchaseToUpdate = await _appDbContext.CreditPurchases.FirstOrDefaultAsync(cp => cp.Id == request.Id);

            if (creditPurchaseToUpdate is null)
            {
                return new AppError("Compra não encontrada.", ErrorTypeEnum.Validation);
            }

            var creditPurchaseToCreate = new CreateCreditPurchaseRequest
                {
                    PurchaseDate = request.PurchaseDate ??
                                   creditPurchaseToUpdate.PurchaseDate,
                    Description = request.Description ??
                                  creditPurchaseToUpdate.Description,
                    TotalInstallment = request.TotalInstallment ?? creditPurchaseToUpdate.TotalInstallment,
                    Destination = request.Destination ??
                                  creditPurchaseToUpdate.Destination,
                    CreditCardId = request.CreditCardId ?? creditPurchaseToUpdate.CreditCardId,
                    TotalAmount = request.TotalAmount is not null
                        ? (double)request.TotalAmount
                        : (double)creditPurchaseToUpdate.TotalAmount,
                    CategoryId = request.CategoryId
                };


            var deleteResult = await this.DeleteCreditPurchaseAsync(creditPurchaseToUpdate.Id, userId);

            if (deleteResult.IsError)
            {
                return deleteResult.Error;
            }

            var result = await this.CreateCreditPurchaseAsync(creditPurchaseToCreate, userId);

            if(result.IsError)
            {
                var undoOldPurchase =
                    await this.CreateCreditPurchaseAsync(_mapper.Map<CreateCreditPurchaseRequest>(creditPurchaseToUpdate),
                        userId);

                if (undoOldPurchase.IsError)
                {
                    return new AppError(
                        $"Houve um erro ao recriar a transação e não foi possível recupera-la, será necessário refaze-la, erro: {result.Error.ErrorMessage}",
                        result.Error.ErrorType);
                }
                return new AppError($"Não foi possível atualizar a transação: {result.Error.ErrorMessage}", result.Error.ErrorType);
            }


            await _appDbContext.SaveChangesAsync();
            return result.Value;
        }

        public async Task<Result<InfoCreditCardResponse[]>> GetCreditCardInfo(int userId)
        {
            var creditCards = await _appDbContext.CreditCards.Include(cc => cc.Account).Where(cc => cc.Account.UserId == userId).ToListAsync();

            return _mapper.Map<InfoCreditCardResponse[]>(creditCards);
        }
    }
}
