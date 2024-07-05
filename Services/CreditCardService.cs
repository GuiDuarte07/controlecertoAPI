using AutoMapper;
using Finantech.DTOs.CreditCard;
using Finantech.DTOs.CreditPurchase;
using Finantech.DTOs.Invoice;
using Finantech.Enums;
using Finantech.Models.AppDbContext;
using Finantech.Models.DTOs;
using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Finantech.Services
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

        public async Task<InfoCreditCardResponse> CreateCreditCardAsync(CreateCreditCardRequest request, int userId)
        {
            var creditCardToCreate = _mapper.Map<CreditCard>(request);
            var account = await _appDbContext.Accounts.Include(a => a.CreditCard).FirstOrDefaultAsync(cd => cd.Id == creditCardToCreate.AccountId)
                ?? throw new Exception("Conta não encontrada.");

            if (account.UserId != userId) throw new Exception("Conta não pertence ao usuário.");

            if (account.CreditCard is not null) throw new Exception("Conta já possui um cartão.");

            var createdCreditCard = await _appDbContext.CreditCards.AddAsync(creditCardToCreate);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoCreditCardResponse>(createdCreditCard.Entity);
        }

        public async Task<InfoCreditCardResponse> UpdateCreditCardAsync(UpdateCreditCardRequest request, int userId)
        {
            var creditCardToUpdate = await _appDbContext
                .CreditCards.Include(cd => cd.Account)
                .FirstOrDefaultAsync(cd => cd.Id == request.Id)
                ?? throw new Exception("Conta não encontrada.");

            if (creditCardToUpdate.Account.UserId != userId) throw new Exception("Conta não pertence ao usuário.");

            creditCardToUpdate.UpdatedAt = DateTime.UtcNow;

            if(request.CardBrand is not null)
                creditCardToUpdate.CardBrand = request.CardBrand;
            if (request.Description is not null)
                creditCardToUpdate.Description = request.Description;



            /*
             * Será feito posteriormente a lógica em mudar o TotalLimit, UsedLimit, DueDate e CloseDate
             */

            /*
                if (request.CloseDay is not null)
                    creditCardToUpdate.CloseDay = (int)request.CloseDay;
                if (request.DueDay is not null)
                    creditCardToUpdate.DueDay = (int)request.DueDay;
                if(request.TotalLimit is not null && request.TotalLimit >= 0)
                    creditCardToUpdate.TotalLimit = (double)request.TotalLimit;
                if (request.UsedLimit is not null && request.UsedLimit >= 0)
                    creditCardToUpdate.UsedLimit = (double)request.UsedLimit;
            */

            var updatedCreditCard = _appDbContext.CreditCards.Update(creditCardToUpdate);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoCreditCardResponse>(updatedCreditCard.Entity);
        }
           

        public async Task<InfoCreditPurchaseResponse> CreateCreditPurchaseAsync(CreateCreditPurchaseRequest request, int userId)
        {
            var creditPurchaseToCreate = _mapper.Map<CreditPurchase>(request);

            var creditCard = await _appDbContext.CreditCards.Include(cc => cc.Account).FirstOrDefaultAsync(cc => cc.Id == request.CreditCardId && cc.Account.UserId == userId)
                ?? throw new Exception("Cartão de Crédito não localizado ou não pertence ao usuário.");

            if(creditCard.TotalLimit - creditCard.UsedLimit < request.TotalAmount)
                throw new Exception("Limite inferior ao valor da compra.");

            var category = await _appDbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == userId)
                ?? throw new Exception("Categoria informada não encontrada");

            if (category.BillType != BillTypeEnum.EXPENSE)
                throw new Exception("Essa categoria não é de gastos.");


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

                    int today = DateTime.Now.Day;
                    bool isInClosingDate = today >= creditCard.CloseDay && today <= creditCard.DueDay;
                    bool isAfterDueDate = today > creditCard.CloseDay;

                    var actualClosingMonthDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, creditCard.CloseDay);
                    var actualDueMonthDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, creditCard.DueDay);


                    ICollection<Transaction> transactions = [];

                    for (int i = 0; i < totalInstallment - installmentsPaid; i++)
                    {
                        var monthClosingInvoiceDate = actualClosingMonthDate.AddMonths(i);

                        // Se essa fatura já está na sua data de fechamento, entra pra próxima.
                        if (isInClosingDate)
                            monthClosingInvoiceDate = monthClosingInvoiceDate.AddMonths(1);

                        if (isAfterDueDate)
                            monthClosingInvoiceDate = monthClosingInvoiceDate.AddMonths(1);

                        var monthInvoice = await _appDbContext.Invoices.FirstOrDefaultAsync
                        (
                            i => i.CreditCard.AccountId.Equals(creditCard.AccountId) && i.ClosingDate.Equals(monthClosingInvoiceDate)
                        );

                        if(monthInvoice is null)
                        {
                            var newInvoice = new Invoice
                            {
                                ClosingDate = monthClosingInvoiceDate,
                                DueDate = monthClosingInvoiceDate.AddDays(7),
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
                            Description = request.Description + $" - parcela {i + 1 + installmentsPaid}/{(totalInstallment - installmentsPaid)}",
                            Destination = request.Destination,
                            PurchaseDate = request.PurchaseDate ?? DateTime.UtcNow,
                        };


                        transactions.Add(newTransaction);
                        monthInvoice.TotalAmount += installmentAmount;
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

        public async Task<IEnumerable<InfoInvoiceResponse>> GetInvoicesWithPaginationAsync(int pageNumber, int pageSize, int userId, DateTime startDate, DateTime endDate, int? accountId)
        {
            int startIndex = (pageNumber - 1) * pageSize;

            if (accountId.HasValue)
            {
                var _ = await _appDbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId) ??
                    throw new Exception("Conta não encontrada.");
            }

            var invoices = await _appDbContext.Invoices.Where(i =>
                i.CreditCard.Account.UserId == userId &&
                i.DueDate >= startDate &&
                i.DueDate <= endDate && 
                i.CreditCard.AccountId == (accountId ?? i.CreditCard.AccountId)
            ).Skip(startIndex)
            .Take(pageSize)
            .OrderByDescending(a => a.DueDate)
            .ToListAsync();            

            return _mapper.Map<ICollection<InfoInvoiceResponse>>(invoices);
        }

        public async Task<InfoInvoicePaymentResponse> PayInvoiceAsync(CreteInvoicePaymentRequest invoicePaymentRequest, int userId)
        {
            var invoicePayment = _mapper.Map<InvoicePayment>(invoicePaymentRequest);

            var invoice = await _appDbContext.Invoices.FirstAsync(
                i => i.Id == invoicePayment.InvoiceId && 
                i.CreditCard.Account.UserId == userId
            ) ?? throw new Exception("Fatura não encontrada.");

            if (DateTime.Now < invoice.ClosingDate)
                throw new Exception("Essa fatura ainda não está fechada ainda.");
            if (invoicePayment.PaymentDate < invoice.DueDate)
                throw new Exception("A data de pagamento da fatura não pode ser anterior a data que a fatura foi gerada.");

            Account? account = null;

            if (invoicePayment.AccountPaidId.HasValue)
            {
                account = await _appDbContext.Accounts.FirstAsync(a => a.Id == invoicePayment.AccountPaidId && a.UserId == userId) ??
                    throw new Exception("Conta não encontrada.");
            }

            if (invoice.IsPaid) throw new Exception("Essa fatura já está paga.");

            var amountRemainder = invoice.TotalAmount - invoice.TotalPaid;

            if (invoicePayment.AmountPaid > amountRemainder) 
                throw new Exception($"Quantidade a ser paga (R$ {invoicePayment.AmountPaid}) é maior que o valor da fatura restante (R$ {amountRemainder}).");

            invoice.TotalPaid += invoicePayment.AmountPaid;

            if (invoice.TotalPaid == invoice.TotalAmount)
                invoice.IsPaid = true;

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (account is not null && !invoicePaymentRequest.JustForRecord)
                    {
                        account.Balance -= invoicePayment.AmountPaid;
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
        public async Task DeleteCreditPurchaseAsync(int purchaseId, int userId)
        {
            var creditPurchaseToDelete = await _appDbContext.CreditPurchases.FirstAsync(cp => cp.Id == purchaseId && cp.CreditCard.Account.UserId == userId) ?? throw new Exception("Compra no cartão não localizado ou não pertence ao usuário.");

            var creditCard = await _appDbContext.CreditCards.Include(cc => cc.Account).FirstOrDefaultAsync(cc => cc.Id == creditPurchaseToDelete.CreditCardId && cc.Account.UserId == userId)
                ?? throw new Exception("Cartão de Crédito não localizado ou não pertence ao usuário.");

            var creditExpenses = await _appDbContext.Transactions.Where(ce => ce.Type == TransactionTypeEnum.CREDITEXPENSE && ce.CreditPurchaseId == creditPurchaseToDelete.Id).ToListAsync();

            

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var expense in creditExpenses)
                    {
                        var invoice = await _appDbContext.Invoices.FirstAsync(cp => cp.Id == expense.InvoiceId);

                        if (invoice.IsPaid)
                        {
                            throw new Exception("Não é mais possível excluir esse registro pois a fatura já foi paga.");
                        }

                        invoice.TotalAmount -= expense.Amount;
                    }

                    creditCard.UsedLimit -= creditPurchaseToDelete.TotalAmount;

                    _appDbContext.Transactions.RemoveRange(creditExpenses);
                    _appDbContext.CreditPurchases.Remove(creditPurchaseToDelete);

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

        public async Task<InfoTransactionResponse[]> GetCreditExpensesFromInvoice(int invoiceId, int userId)
        {
            var creditExpenses = await _appDbContext.Transactions.Where(ce =>ce.Type == TransactionTypeEnum.CREDITEXPENSE && ce.InvoiceId == invoiceId && ce.Account.UserId == userId).ToListAsync();

            return _mapper.Map<InfoTransactionResponse[]>(creditExpenses);
        }

        public Task<InfoCreditPurchaseResponse> UpdateCreditPurchaseAsync(UpdateCreditPurchaseResponse request, int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<InfoCreditCardResponse[]> GetCreditCardInfo(int userId)
        {
            var creditCards = await _appDbContext.CreditCards.Include(cc => cc.Account).Where(cc => cc.Account.UserId == userId).ToListAsync();

            return _mapper.Map<InfoCreditCardResponse[]>(creditCards);
        }
    }
}
