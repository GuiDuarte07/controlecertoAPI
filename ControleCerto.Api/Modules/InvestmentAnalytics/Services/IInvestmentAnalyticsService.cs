using ControleCerto.Errors;
using ControleCerto.Modules.InvestmentAnalytics.DTOs;

namespace ControleCerto.Modules.InvestmentAnalytics.Services;

public interface IInvestmentAnalyticsService
{
    Task<Result<InvestmentAnalyticsResponse>> GetAsync(
        InvestmentAnalyticsRequest request,
        int userId,
        CancellationToken cancellationToken = default);
}
