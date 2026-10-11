namespace ControleCerto.Modules.InvestmentAnalytics.DTOs;

public sealed class InvestmentAnalyticsRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public long? InvestmentId { get; set; }
}
