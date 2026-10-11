using ControleCerto.Models.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ControleCerto.Modules.CreditCards.Services;

public static class CreditCardBalanceService
{
    public static async Task<IDbContextTransaction> BeginTransactionAsync(AppDbContext context, int userId)
    {
        var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Serialize card operations for this owner before reading balances or payments.
            if (context.Database.IsNpgsql())
                await context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(73421, {userId})");
            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }

    public static async Task ReconcileAsync(AppDbContext context, int userId)
    {
        var cards = await context.CreditCards
            .Where(c => c.Account.UserId == userId)
            .ToListAsync();
        var balances = await context.Invoices
            .Where(i => i.CreditCard.Account.UserId == userId)
            .Select(i => new { i.CreditCardId, i.TotalAmount, i.TotalPaid })
            .ToListAsync();
        var usedByCard = balances.GroupBy(i => i.CreditCardId)
            .ToDictionary(g => g.Key, g => Math.Round(g.Sum(i => Math.Max(0, Math.Round(i.TotalAmount - i.TotalPaid, 2))), 2));

        foreach (var card in cards)
            card.UsedLimit = usedByCard.GetValueOrDefault(card.Id);

        await context.SaveChangesAsync();
    }
}
