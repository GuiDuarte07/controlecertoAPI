using AutoMapper;
using Finantech.DTOs.CreditCard;
using Finantech.DTOs.CreditPurcchase;
using Finantech.DTOs.Expense;
using Finantech.Migrations;
using Finantech.Models.AppDbContext;
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
            if(request.Number is not null)
                creditCardToUpdate.Number = request.Number;
            if (request.DueDay is not null)
                creditCardToUpdate.DueDay = (int)request.DueDay;
            if (request.CloseDay is not null)
                creditCardToUpdate.CloseDay = (int)request.CloseDay;
            if (request.Description is not null)
                creditCardToUpdate.Description = request.Description;
            if(request.TotalLimit is not null && request.TotalLimit >= 0)
                creditCardToUpdate.TotalLimit = (double)request.TotalLimit;
            if (request.UsedLimit is not null && request.UsedLimit >= 0)
                creditCardToUpdate.UsedLimit = (double)request.UsedLimit;

            var updatedCreditCard = _appDbContext.CreditCards.Update(creditCardToUpdate);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoCreditCardResponse>(updatedCreditCard);
        }

        public async Task<InfoCreditPurchaseResponse> CreateCreditPurchaseAsync(CreateCreditPurchaseRequest request, int userId)
        {
            /*
             * IGNORAR CATEGORY NO MAPPER ======================================================================
             */
            var creditPurchaseToCreate = _mapper.Map<CreditPurchase>(request);

            var creditCard = await _appDbContext.CreditCards.Include(cc => cc.Account).FirstOrDefaultAsync(cc => cc.Id == request.CreditCardId && cc.Account.UserId == userId)
                ?? throw new Exception("Cartão de Crédito não localizado ou não pertence ao usuário.");

            if(creditCard.TotalLimit - creditCard.UsedLimit < request.TotalAmount)
                throw new Exception("Limite inferior ao valor da compra.");
            
            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {
                    double TotalAmount = request.TotalAmount;
                    int TotalInstallment = request.TotalInstallment;
                    int InstallmentsPaid = request.InstallmentsPaid;
                    double installmentAmount = TotalAmount / (request.TotalInstallment - InstallmentsPaid);

                    /*
                     * Detalhes sobre implementação:
                     * - Se a data de fechamento da fatura é dia 20 p.e., as compras do dia 20 a 27 entram na fatura do próximo mês
                     * - Se a compra foi feito antes do fechamento, ex: dia 19, a primeira parcela da compra já entra na fatura do mês
                     * * Por enquanto estamos supondo que o dia de fechamento possa ser entre dia 8 e 28 ---------
                     *   (ou seja, não corre o risco de a fatura do mês acaber sendo paga no começo do outro mês).
                     */

                    var createdCreditPurchase = await _appDbContext.CreditPurchases.AddAsync(creditPurchaseToCreate);
                    await _appDbContext.SaveChangesAsync();

                    int today = DateTime.Now.Day;
                    bool isInClosingDate = today >= creditCard.CloseDay && today <= creditCard.DueDay;
                    bool isAfterDueDate = today > creditCard.CloseDay;

                    var actualClosingMonthDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, creditCard.CloseDay);
                    var actualDueMonthDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, creditCard.DueDay);


                    ICollection<CreditExpense> expenses = [];

                    for (int i = 0; i < TotalAmount - InstallmentsPaid; i++)
                    {
                        var monthClosingInvoiceDate = actualClosingMonthDate.AddMonths(i);

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
                                DueDate = monthClosingInvoiceDate,
                                ClosingDate = monthClosingInvoiceDate.AddDays(7),
                                CreditCardId = creditCard.Id,
                            };

                            var invoiceCreated = await _appDbContext.Invoices.AddAsync(newInvoice);
                            await _appDbContext.SaveChangesAsync();

                            monthInvoice = invoiceCreated.Entity;
                        }
                        

                        var expense = new CreditExpense
                        {
                            CreditPurchaseId = createdCreditPurchase.Entity.Id,
                            AccountId = creditCard.AccountId,
                            CategoryId = request.CategoryId,
                            Amount = installmentAmount,
                            InstallmentNumber = i + 1 + InstallmentsPaid,
                            InvoiceId = monthInvoice.Id,
                            Description = request.Description + $" - parcela {i + 1 + InstallmentsPaid}/{(TotalAmount - InstallmentsPaid)}",
                            Destination = request.Destination,
                            PurchaseDate = request.PurchaseDate ?? DateTime.UtcNow,
                        };


                        expenses.Add(expense);
                        monthInvoice.TotalAmount += installmentAmount;
                        _appDbContext.Invoices.Update(monthInvoice);
                    }


                    creditCard.UsedLimit += request.TotalAmount;
                    await _appDbContext.CreditExpenses.AddRangeAsync(expenses);
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

        public Task DeleteCreditPurchaseAsync(int purchaseId, int userId)
        {
            throw new NotImplementedException();
        }

        

        public Task<InfoCreditPurchaseResponse> UpdateCreditPurchaseAsync(UpdateCreditPurchaseResponse request, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
