namespace ControleCerto.Modules.TransactionAnalytics.DTOs;

public sealed class TransactionAnalyticsRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string CreditDateBasis { get; set; } = "invoice";
}
