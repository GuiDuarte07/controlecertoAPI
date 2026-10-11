using ControleCerto.Decorators;
using ControleCerto.Extensions;
using ControleCerto.Modules.TransactionAnalytics.DTOs;
using ControleCerto.Modules.TransactionAnalytics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Modules.TransactionAnalytics.Controllers;

[ApiController]
[Route("api/transactions/analytics")]
[Authorize]
[ExtractTokenInfo]
public sealed class TransactionAnalyticsController(ITransactionAnalyticsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] TransactionAnalyticsRequest request, CancellationToken cancellationToken)
    {
        var userId = (int)HttpContext.Items["UserId"]!;
        return (await service.GetAsync(request, userId, cancellationToken)).HandleReturnResult();
    }
}
