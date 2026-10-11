using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Modules.TransactionAnalytics.DTOs;
using ControleCerto.Modules.TransactionAnalytics.Validations;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Modules.TransactionAnalytics.Services;

public sealed class TransactionAnalyticsService(AppDbContext context) : ITransactionAnalyticsService
{
    public async Task<Result<TransactionAnalyticsResponse>> GetAsync(
        TransactionAnalyticsRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var validation = new TransactionAnalyticsRequestValidator().Validate(request);
        if (!validation.IsValid)
            return new AppError(validation.Errors[0].ErrorMessage, ErrorTypeEnum.Validation);

        var start = DateTime.SpecifyKind(request.StartDate.Date, DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(request.EndDate.Date, DateTimeKind.Utc);
        var previousStart = start.AddDays(-((end - start).Days + 1));
        var exclusiveEnd = end.AddDays(1);
        var byInvoice = request.CreditDateBasis == "invoice";

        // Invoice payments and transfers are not expenses or income. Card installments count once.
        var query = context.Transactions.AsNoTracking()
            .Where(t => t.Account.UserId == userId &&
                (t.Type == TransactionTypeEnum.INCOME || t.Type == TransactionTypeEnum.EXPENSE ||
                 t.Type == TransactionTypeEnum.CREDITEXPENSE))
            .Select(t => new
            {
                Transaction = t,
                EffectiveDate = byInvoice && t.Type == TransactionTypeEnum.CREDITEXPENSE && t.Invoice != null
                    ? t.Invoice.InvoiceDate : t.PurchaseDate,
            })
            .Where(t => t.EffectiveDate >= previousStart && t.EffectiveDate < exclusiveEnd);

        var transactions = await query.OrderByDescending(t => t.EffectiveDate).ThenByDescending(t => t.Transaction.Id)
            .Select(t => new AnalyticsTransactionResponse
            {
                Id = t.Transaction.Id,
                Type = t.Transaction.Type == TransactionTypeEnum.INCOME ? "INCOME" :
                    t.Transaction.Type == TransactionTypeEnum.CREDITEXPENSE ? "CREDITEXPENSE" : "EXPENSE",
                Amount = t.Transaction.Amount,
                Date = t.EffectiveDate,
                TransactionDate = t.Transaction.PurchaseDate,
                Description = t.Transaction.Description,
                Destination = t.Transaction.Destination,
                JustForRecord = t.Transaction.JustForRecord,
                AccountId = t.Transaction.AccountId,
                AccountName = t.Transaction.Account.Bank,
                CardId = t.Transaction.CreditPurchase != null ? t.Transaction.CreditPurchase.CreditCardId : null,
                CardName = t.Transaction.CreditPurchase != null ? t.Transaction.CreditPurchase.CreditCard.Description : null,
                CategoryId = t.Transaction.CategoryId,
                CategoryName = t.Transaction.Category.Name,
                CategoryColor = t.Transaction.Category.Color,
                ParentCategoryId = t.Transaction.Category.ParentId,
                ParentCategoryName = t.Transaction.Category.Parent != null ? t.Transaction.Category.Parent.Name : null,
                ParentCategoryColor = t.Transaction.Category.Parent != null ? t.Transaction.Category.Parent.Color : null,
                InvoiceId = t.Transaction.InvoiceId,
                InvoiceDate = t.Transaction.Invoice != null ? t.Transaction.Invoice.InvoiceDate : null,
            }).ToListAsync(cancellationToken);

        return new TransactionAnalyticsResponse
        {
            StartDate = start, EndDate = end, PreviousStartDate = previousStart,
            PreviousEndDate = start.AddDays(-1), Transactions = transactions,
        };
    }
}
