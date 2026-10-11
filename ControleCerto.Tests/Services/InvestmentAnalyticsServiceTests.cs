using ControleCerto.Enums;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Modules.InvestmentAnalytics.DTOs;
using ControleCerto.Modules.InvestmentAnalytics.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ControleCerto.Tests.Services;

public sealed class InvestmentAnalyticsServiceTests
{
    [Fact]
    public async Task GetAsync_GroupsMultipleAdjustmentsAndContributionsByMonth()
    {
        await using var context = CreateContext();
        var investment = new Investment { Id = 1, Name = "CDB", UserId = 7, CurrentValue = 1720 };
        context.Investments.Add(investment);
        context.InvestmentHistories.AddRange(
            History(1, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 1000, InvestmentHistoryTypeEnum.INITIAL_BALANCE),
            History(1, new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), 10, InvestmentHistoryTypeEnum.ADJUSTMENT),
            History(1, new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc), 500, InvestmentHistoryTypeEnum.INVEST),
            History(1, new DateTime(2026, 1, 25, 0, 0, 0, DateTimeKind.Utc), 15, InvestmentHistoryTypeEnum.ADJUSTMENT),
            History(1, new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc), -5, InvestmentHistoryTypeEnum.ADJUSTMENT),
            History(1, new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc), 200, InvestmentHistoryTypeEnum.INVEST));
        await context.SaveChangesAsync();
        var service = new InvestmentAnalyticsService(context);

        var result = await service.GetAsync(new InvestmentAnalyticsRequest
        {
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 2, 28),
        }, 7);

        result.IsSuccess.Should().BeTrue();
        result.Value.Monthly.Should().HaveCount(2);
        result.Value.Monthly[0].Contributions.Should().Be(1500);
        result.Value.Monthly[0].Earnings.Should().Be(25);
        result.Value.Monthly[0].ClosingBalance.Should().Be(1525);
        result.Value.Monthly[1].Contributions.Should().Be(200);
        result.Value.Monthly[1].Earnings.Should().Be(-5);
        result.Value.Monthly[1].ClosingBalance.Should().Be(1720);
        result.Value.Summary.Earnings.Should().Be(20);
        result.Value.Summary.AverageMonthlyEarnings.Should().Be(10);
        result.Value.Summary.PositiveMonths.Should().Be(1);
        result.Value.Summary.NegativeMonths.Should().Be(1);
    }

    [Fact]
    public async Task GetAsync_WithInvestmentFilter_ExcludesOtherInvestments()
    {
        await using var context = CreateContext();
        context.Investments.AddRange(
            new Investment { Id = 1, Name = "CDB", UserId = 7, CurrentValue = 110 },
            new Investment { Id = 2, Name = "Tesouro", UserId = 7, CurrentValue = 550 });
        context.InvestmentHistories.AddRange(
            History(1, new DateTime(2026, 1, 1), 100, InvestmentHistoryTypeEnum.INITIAL_BALANCE),
            History(1, new DateTime(2026, 1, 31), 10, InvestmentHistoryTypeEnum.ADJUSTMENT),
            History(2, new DateTime(2026, 1, 1), 500, InvestmentHistoryTypeEnum.INITIAL_BALANCE),
            History(2, new DateTime(2026, 1, 31), 50, InvestmentHistoryTypeEnum.ADJUSTMENT));
        await context.SaveChangesAsync();
        var service = new InvestmentAnalyticsService(context);

        var result = await service.GetAsync(new InvestmentAnalyticsRequest
        {
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            InvestmentId = 1,
        }, 7);

        result.IsSuccess.Should().BeTrue();
        result.Value.Investments.Should().ContainSingle().Which.Name.Should().Be("CDB");
        result.Value.Summary.EndingBalance.Should().Be(110);
        result.Value.Summary.Earnings.Should().Be(10);
    }

    private static InvestmentHistory History(long investmentId, DateTime date, double change, InvestmentHistoryTypeEnum type) => new()
    {
        InvestmentId = investmentId,
        OccurredAt = date,
        ChangeAmount = change,
        TotalValue = 0,
        Type = type,
    };

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"InvestmentAnalytics_{Guid.NewGuid():N}")
            .Options;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:WebApiDatabase"] = "Test",
        }).Build();
        return new AppDbContext(options, configuration);
    }
}
