using ControleCerto.Decorators;
using ControleCerto.DTOs.Investment;
using ControleCerto.Errors;
using ControleCerto.Extensions;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/investments")]
    [Authorize]
    [ExtractTokenInfo]
    public class InvestmentController : ControllerBase
    {
        private readonly IInvestmentService _investmentService;

        public InvestmentController(IInvestmentService investmentService)
        {
            _investmentService = investmentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvestment([FromBody] CreateInvestmentRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.CreateInvestmentAsync(request, userId);
            if (result.IsSuccess) return Created($"/api/investments/{result.Value.Id}", result.Value);
            return result.HandleReturnResult();
        }

        [HttpPatch("{investmentId:long}")]
        public async Task<IActionResult> UpdateInvestment([FromRoute] long investmentId, [FromBody] UpdateInvestmentRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            request.Id = investmentId;
            ModelState.Clear();
            TryValidateModel(request);

            if (!ModelState.IsValid)
            {
                var errorResponse = ErrorResponse.FromModelState(ModelState);
                return StatusCode(errorResponse.Code, errorResponse);
            }

            var result = await _investmentService.UpdateInvestmentAsync(request, userId);
            return result.HandleReturnResult();
        }

        [HttpPost("{investmentId:long}/deposit")]
        public async Task<IActionResult> Deposit([FromRoute] long investmentId, [FromBody] DepositInvestmentRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            request.InvestmentId = investmentId;
            var result = await _investmentService.DepositAsync(request, userId);
            if (result.IsSuccess) return Created($"/api/investments/{result.Value.Id}", result.Value);
            return result.HandleReturnResult();
        }

        [HttpPost("{investmentId:long}/withdraw")]
        public async Task<IActionResult> Withdraw([FromRoute] long investmentId, [FromBody] DepositInvestmentRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            request.InvestmentId = investmentId;
            var result = await _investmentService.WithdrawAsync(request, userId);
            if (result.IsSuccess) return Created($"/api/investments/{result.Value.Id}", result.Value);
            return result.HandleReturnResult();
        }

        [HttpPost("{investmentId:long}/adjust")]
        public async Task<IActionResult> AdjustValue([FromRoute] long investmentId, [FromBody] AdjustInvestmentRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            request.InvestmentId = investmentId;
            var result = await _investmentService.AdjustInvestmentAsync(request, userId);
            if (result.IsSuccess) return Created($"/api/investments/{result.Value.Id}", result.Value);
            return result.HandleReturnResult();
        }

        [HttpDelete("{investmentId:long}")]
        public async Task<IActionResult> DeleteInvestment([FromRoute] long investmentId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.DeleteInvestmentAsync(investmentId, userId);
            return result.HandleReturnResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetInvestments()
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.GetInvestmentsAsync(userId);
            return result.HandleReturnResult();
        }

        [HttpGet("{investmentId:long}")]
        public async Task<IActionResult> GetInvestment([FromRoute] long investmentId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.GetInvestmentAsync(investmentId, userId);
            return result.HandleReturnResult();
        }

        [HttpGet("{investmentId:long}/history")]
        public async Task<IActionResult> GetInvestmentHistory([FromRoute] long investmentId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.GetInvestmentHistoryAsync(investmentId, userId);
            return result.HandleReturnResult();
        }
    }
}
