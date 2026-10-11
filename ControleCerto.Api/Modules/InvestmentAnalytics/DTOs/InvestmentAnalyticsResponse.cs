namespace ControleCerto.Modules.InvestmentAnalytics.DTOs;

public sealed class InvestmentAnalyticsResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public InvestmentAnalyticsSummaryResponse Summary { get; set; } = new();
    public List<InvestmentMonthlyAnalyticsResponse> Monthly { get; set; } = [];
    public List<InvestmentPerformanceResponse> Investments { get; set; } = [];
}

public sealed class InvestmentAnalyticsSummaryResponse
{
    public double OpeningBalance { get; set; }
    public double EndingBalance { get; set; }
    public double Contributions { get; set; }
    public double Withdrawals { get; set; }
    public double Earnings { get; set; }
    public double AverageBalance { get; set; }
    public double ReturnRate { get; set; }
    public double AverageMonthlyContribution { get; set; }
    public double AverageMonthlyEarnings { get; set; }
    public int PositiveMonths { get; set; }
    public int NegativeMonths { get; set; }
}

public sealed class InvestmentMonthlyAnalyticsResponse
{
    public DateTime Month { get; set; }
    public double OpeningBalance { get; set; }
    public double Contributions { get; set; }
    public double Withdrawals { get; set; }
    public double NetContribution { get; set; }
    public double Earnings { get; set; }
    public double ClosingBalance { get; set; }
    public double AverageBalance { get; set; }
    public double ReturnRate { get; set; }
    public double AccumulatedContributions { get; set; }
    public double AccumulatedEarnings { get; set; }
}

public sealed class InvestmentPerformanceResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public double OpeningBalance { get; set; }
    public double EndingBalance { get; set; }
    public double Contributions { get; set; }
    public double Withdrawals { get; set; }
    public double Earnings { get; set; }
    public double AverageBalance { get; set; }
    public double ReturnRate { get; set; }
}
