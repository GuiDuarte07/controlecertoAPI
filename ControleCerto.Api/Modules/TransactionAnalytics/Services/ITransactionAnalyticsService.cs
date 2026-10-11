using ControleCerto.Errors;
using ControleCerto.Modules.TransactionAnalytics.DTOs;

namespace ControleCerto.Modules.TransactionAnalytics.Services;

public interface ITransactionAnalyticsService
{
    Task<Result<TransactionAnalyticsResponse>> GetAsync(
        TransactionAnalyticsRequest request,
        int userId,
        CancellationToken cancellationToken = default);
}
