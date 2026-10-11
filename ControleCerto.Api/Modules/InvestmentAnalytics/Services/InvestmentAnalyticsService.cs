using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Modules.InvestmentAnalytics.DTOs;
using ControleCerto.Modules.InvestmentAnalytics.Validations;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Modules.InvestmentAnalytics.Services;

public sealed class InvestmentAnalyticsService(AppDbContext context) : IInvestmentAnalyticsService
{
    public async Task<Result<InvestmentAnalyticsResponse>> GetAsync(
        InvestmentAnalyticsRequest request,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var validation = new InvestmentAnalyticsRequestValidator().Validate(request);
        if (!validation.IsValid)
            return new AppError(validation.Errors[0].ErrorMessage, ErrorTypeEnum.Validation);

        var start = DateTime.SpecifyKind(request.StartDate.Date, DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(request.EndDate.Date, DateTimeKind.Utc);
        var exclusiveEnd = end.AddDays(1);

        var investmentData = await context.Investments.AsNoTracking()
            .Where(i => i.UserId == userId && (!request.InvestmentId.HasValue || i.Id == request.InvestmentId.Value))
            .OrderBy(i => i.Name)
            .Select(i => new { i.Id, i.Name })
            .ToListAsync(cancellationToken);
        var investments = investmentData.Select(i => new InvestmentRow(i.Id, i.Name)).ToList();

        if (request.InvestmentId.HasValue && investments.Count == 0)
            return new AppError("Investimento não encontrado.", ErrorTypeEnum.NotFound);

        var investmentIds = investments.Select(i => i.Id).ToList();
        var historyData = await context.InvestmentHistories.AsNoTracking()
            .Where(h => investmentIds.Contains(h.InvestmentId) && h.OccurredAt < exclusiveEnd)
            .OrderBy(h => h.OccurredAt)
            .ThenBy(h => h.InvestmentId)
            .ThenBy(h => h.Id)
            .Select(h => new { h.InvestmentId, h.OccurredAt, h.ChangeAmount, h.Type })
            .ToListAsync(cancellationToken);
        var events = historyData
            .Select(h => new HistoryRow(h.InvestmentId, h.OccurredAt, h.ChangeAmount, h.Type))
            .ToList();

        var monthly = CalculateMonthly(events, start, exclusiveEnd);
        var performance = investments.Select(investment =>
            CalculateInvestment(investment, events.Where(e => e.InvestmentId == investment.Id).ToList(), start, exclusiveEnd))
            .OrderByDescending(i => i.EndingBalance)
            .ToList();
        var averageBalance = WeightedAverage(events, start, exclusiveEnd);
        var earnings = monthly.Sum(m => m.Earnings);

        return new InvestmentAnalyticsResponse
        {
            StartDate = start,
            EndDate = end,
            Monthly = monthly,
            Investments = performance,
            Summary = new InvestmentAnalyticsSummaryResponse
            {
                OpeningBalance = Money(BalanceBefore(events, start)),
                EndingBalance = Money(BalanceBefore(events, exclusiveEnd)),
                Contributions = Money(monthly.Sum(m => m.Contributions)),
                Withdrawals = Money(monthly.Sum(m => m.Withdrawals)),
                Earnings = Money(earnings),
                AverageBalance = Money(averageBalance),
                ReturnRate = Rate(earnings, averageBalance),
                AverageMonthlyContribution = Money(monthly.Count == 0 ? 0 : monthly.Average(m => m.Contributions)),
                AverageMonthlyEarnings = Money(monthly.Count == 0 ? 0 : monthly.Average(m => m.Earnings)),
                PositiveMonths = monthly.Count(m => m.Earnings > 0),
                NegativeMonths = monthly.Count(m => m.Earnings < 0),
            },
        };
    }

    private static List<InvestmentMonthlyAnalyticsResponse> CalculateMonthly(
        List<HistoryRow> events, DateTime start, DateTime exclusiveEnd)
    {
        var result = new List<InvestmentMonthlyAnalyticsResponse>();
        var month = new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        double accumulatedContributions = 0;
        double accumulatedEarnings = 0;

        while (month < exclusiveEnd)
        {
            var periodStart = month < start ? start : month;
            var nextMonth = month.AddMonths(1);
            var periodEnd = nextMonth > exclusiveEnd ? exclusiveEnd : nextMonth;
            var periodEvents = events.Where(e => e.OccurredAt >= periodStart && e.OccurredAt < periodEnd).ToList();
            var contributions = periodEvents.Where(IsContribution).Sum(e => Math.Max(0, e.ChangeAmount));
            var withdrawals = periodEvents.Where(e => e.Type == InvestmentHistoryTypeEnum.WITHDRAW).Sum(e => Math.Abs(e.ChangeAmount));
            var earnings = periodEvents.Where(IsEarning).Sum(e => e.ChangeAmount);
            var averageBalance = WeightedAverage(events, periodStart, periodEnd);
            accumulatedContributions += contributions - withdrawals;
            accumulatedEarnings += earnings;

            result.Add(new InvestmentMonthlyAnalyticsResponse
            {
                Month = month,
                OpeningBalance = Money(BalanceBefore(events, periodStart)),
                Contributions = Money(contributions),
                Withdrawals = Money(withdrawals),
                NetContribution = Money(contributions - withdrawals),
                Earnings = Money(earnings),
                ClosingBalance = Money(BalanceBefore(events, periodEnd)),
                AverageBalance = Money(averageBalance),
                ReturnRate = Rate(earnings, averageBalance),
                AccumulatedContributions = Money(accumulatedContributions),
                AccumulatedEarnings = Money(accumulatedEarnings),
            });
            month = nextMonth;
        }

        return result;
    }

    private static InvestmentPerformanceResponse CalculateInvestment(
        InvestmentRow investment, List<HistoryRow> events, DateTime start, DateTime exclusiveEnd)
    {
        var periodEvents = events.Where(e => e.OccurredAt >= start && e.OccurredAt < exclusiveEnd).ToList();
        var contributions = periodEvents.Where(IsContribution).Sum(e => Math.Max(0, e.ChangeAmount));
        var withdrawals = periodEvents.Where(e => e.Type == InvestmentHistoryTypeEnum.WITHDRAW).Sum(e => Math.Abs(e.ChangeAmount));
        var earnings = periodEvents.Where(IsEarning).Sum(e => e.ChangeAmount);
        var averageBalance = WeightedAverage(events, start, exclusiveEnd);
        return new InvestmentPerformanceResponse
        {
            Id = investment.Id,
            Name = investment.Name,
            OpeningBalance = Money(BalanceBefore(events, start)),
            EndingBalance = Money(BalanceBefore(events, exclusiveEnd)),
            Contributions = Money(contributions),
            Withdrawals = Money(withdrawals),
            Earnings = Money(earnings),
            AverageBalance = Money(averageBalance),
            ReturnRate = Rate(earnings, averageBalance),
        };
    }

    private static double WeightedAverage(List<HistoryRow> events, DateTime start, DateTime end)
    {
        var duration = (end - start).TotalSeconds;
        if (duration <= 0) return 0;
        var balance = BalanceBefore(events, start);
        var cursor = start;
        double weighted = 0;
        foreach (var item in events.Where(e => e.OccurredAt >= start && e.OccurredAt < end))
        {
            var eventDate = item.OccurredAt < cursor ? cursor : item.OccurredAt;
            weighted += balance * (eventDate - cursor).TotalSeconds;
            balance += item.ChangeAmount;
            cursor = eventDate;
        }
        weighted += balance * (end - cursor).TotalSeconds;
        return weighted / duration;
    }

    private static double BalanceBefore(IEnumerable<HistoryRow> events, DateTime date) =>
        events.Where(e => e.OccurredAt < date).Sum(e => e.ChangeAmount);

    private static bool IsContribution(HistoryRow item) =>
        item.Type is InvestmentHistoryTypeEnum.INITIAL_BALANCE or InvestmentHistoryTypeEnum.INVEST;

    private static bool IsEarning(HistoryRow item) =>
        item.Type is InvestmentHistoryTypeEnum.ADJUSTMENT or InvestmentHistoryTypeEnum.YIELD;

    private static double Money(double value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    private static double Rate(double earnings, double averageBalance) =>
        averageBalance > 0 ? Math.Round(earnings / averageBalance * 100, 4, MidpointRounding.AwayFromZero) : 0;

    private sealed record InvestmentRow(long Id, string Name);
    private sealed record HistoryRow(long InvestmentId, DateTime OccurredAt, double ChangeAmount, InvestmentHistoryTypeEnum Type);
}
