using ControleCerto.Decorators;
using ControleCerto.Extensions;
using ControleCerto.Modules.InvestmentAnalytics.DTOs;
using ControleCerto.Modules.InvestmentAnalytics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Modules.InvestmentAnalytics.Controllers;

[ApiController]
[Route("api/investments/analytics")]
[Authorize]
[ExtractTokenInfo]
public sealed class InvestmentAnalyticsController(IInvestmentAnalyticsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] InvestmentAnalyticsRequest request, CancellationToken cancellationToken)
    {
        var userId = (int)HttpContext.Items["UserId"]!;
        return (await service.GetAsync(request, userId, cancellationToken)).HandleReturnResult();
    }
}
