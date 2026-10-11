namespace ControleCerto.Modules.TransactionAnalytics.DTOs;

public sealed class TransactionAnalyticsResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PreviousStartDate { get; set; }
    public DateTime PreviousEndDate { get; set; }
    public List<AnalyticsTransactionResponse> Transactions { get; set; } = [];
}

public sealed class AnalyticsTransactionResponse
{
    public long Id { get; set; }
    public string Type { get; set; } = "";
    public double Amount { get; set; }
    public DateTime Date { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = "";
    public string? Destination { get; set; }
    public bool JustForRecord { get; set; }
    public long AccountId { get; set; }
    public string AccountName { get; set; } = "";
    public long? CardId { get; set; }
    public string? CardName { get; set; }
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string CategoryColor { get; set; } = "";
    public long? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string? ParentCategoryColor { get; set; }
    public long? InvoiceId { get; set; }
    public DateTime? InvoiceDate { get; set; }
}
