using AutoMapper;
using ControleCerto.DTOs.CreditPurchase;
using ControleCerto.DTOs.Invoice;
using ControleCerto.Enums;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Profiles;
using ControleCerto.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace ControleCerto.Tests.Services;

public sealed class CreditCardServiceTests : IDisposable
{
    private readonly AppDbContext context;
    private readonly CreditCardService service;

    public CreditCardServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new AppDbContext(options, new ConfigurationBuilder().Build());
        var mapper = new MapperConfiguration(
            configuration => configuration.AddProfile<MapperProfile>(),
            NullLoggerFactory.Instance).CreateMapper();
        service = new CreditCardService(context, mapper);
        context.Users.Add(new User { Id = 1, Name = "Test", Email = "test@example.com", PasswordHash = "test" });
        context.Accounts.Add(new Account { Id = 1, UserId = 1, Balance = 1000, Bank = "Test", Color = "#000000" });
        context.CreditCards.Add(new CreditCard { Id = 1, AccountId = 1, TotalLimit = 1000, Description = "Test", CloseDay = 20, DueDay = 27 });
        context.Categories.Add(new Category("Test", "leaf", "#000000", BillTypeEnum.EXPENSE, 1) { Id = 1 });
        context.SaveChanges();
    }

    [Theory]
    [InlineData(100, 3, 0)]
    [InlineData(100, 6, 0)]
    [InlineData(100, 6, 2)]
    [InlineData(0.05, 3, 0)]
    public async Task InstallmentsSumToPurchaseAndPaymentsRestoreExactlyTheLimit(double amount, int installments, int paid)
    {
        var purchase = await service.CreateCreditPurchaseAsync(new CreateCreditPurchaseRequest
        {
            CreditCardId = 1, CategoryId = 1, TotalAmount = amount, TotalInstallment = installments,
            InstallmentsPaid = paid, PurchaseDate = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
            Description = "Purchase", Destination = "Store",
        }, 1);
        Assert.True(purchase.IsSuccess);
        var invoices = await context.Invoices.OrderBy(i => i.InvoiceDate).ToListAsync();
        Assert.Equal(installments - paid, invoices.Count);
        Assert.Equal(amount, Math.Round(invoices.Sum(i => i.TotalAmount), 2));
        Assert.Equal(amount, (await context.CreditCards.SingleAsync()).UsedLimit);

        foreach (var invoice in invoices)
        {
            var payment = await service.PayInvoiceAsync(Payment(invoice.Id, invoice.TotalAmount), 1);
            Assert.True(payment.IsSuccess);
            Assert.True(invoice.IsPaid);
            Assert.True((await context.CreditCards.SingleAsync()).UsedLimit >= 0);
        }
        Assert.Equal(0, (await context.CreditCards.SingleAsync()).UsedLimit);
        Assert.Equal(1000 - amount, (await context.Accounts.SingleAsync()).Balance, 2);
    }

    [Theory]
    [InlineData(-50)]
    [InlineData(900)]
    public async Task ReadingCardsRepairsDriftAndIncludesFutureInvoices(double corruptedLimit)
    {
        var card = await context.CreditCards.SingleAsync();
        card.UsedLimit = corruptedLimit;
        context.Invoices.AddRange(
            new Invoice { CreditCardId = 1, TotalAmount = 100, TotalPaid = 100, IsPaid = true },
            new Invoice { CreditCardId = 1, TotalAmount = 200, TotalPaid = 50, InvoiceDate = DateTime.UtcNow.AddMonths(3) });
        await context.SaveChangesAsync();
        var result = await service.GetCreditCardInfo(1);
        Assert.True(result.IsSuccess);
        Assert.Equal(150, Assert.Single(result.Value).UsedLimit);
        context.ChangeTracker.Clear();
        Assert.Equal(150, (await context.CreditCards.SingleAsync()).UsedLimit);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeletingPaymentRestoresDebtAndOnlyReversesActualAccountDebit(bool justForRecord)
    {
        var invoice = new Invoice { CreditCardId = 1, TotalAmount = 100 };
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();
        var request = Payment(invoice.Id, 100);
        request.JustForRecord = justForRecord;
        var payment = await service.PayInvoiceAsync(request, 1);
        Assert.True(payment.IsSuccess);
        Assert.Equal(justForRecord ? 1000 : 900, (await context.Accounts.SingleAsync()).Balance);
        Assert.True((await service.DeleteInvoicePaymentAsync(payment.Value.Id, 1)).IsSuccess);
        Assert.Equal(1000, (await context.Accounts.SingleAsync()).Balance);
        Assert.Equal(100, (await context.CreditCards.SingleAsync()).UsedLimit);
        Assert.False(invoice.IsPaid);
        Assert.Equal(0, invoice.TotalPaid);
    }

    [Fact]
    public async Task PaidInvoiceRejectsAnotherPayment()
    {
        var invoice = new Invoice { CreditCardId = 1, TotalAmount = 100 };
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();
        Assert.True((await service.PayInvoiceAsync(Payment(invoice.Id, 100), 1)).IsSuccess);
        Assert.True((await service.PayInvoiceAsync(Payment(invoice.Id, 100), 1)).IsError);
        Assert.Single(await context.InvoicePayments.ToListAsync());
        Assert.Equal(900, (await context.Accounts.SingleAsync()).Balance);
    }

    [Fact]
    public async Task AnotherUserCannotDeletePayment()
    {
        var invoice = new Invoice { CreditCardId = 1, TotalAmount = 100 };
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();
        var payment = await service.PayInvoiceAsync(Payment(invoice.Id, 100), 1);
        Assert.True((await service.DeleteInvoicePaymentAsync(payment.Value.Id, 2)).IsError);
        Assert.Single(await context.InvoicePayments.ToListAsync());
        Assert.Equal(900, (await context.Accounts.SingleAsync()).Balance);
    }

    private static CreteInvoicePaymentRequest Payment(long invoiceId, double amount) => new()
    {
        InvoiceId = invoiceId, AccountId = 1, AmountPaid = amount,
        Description = "Payment", PaymentDate = DateTime.UtcNow,
    };

    public void Dispose() => context.Dispose();
}
