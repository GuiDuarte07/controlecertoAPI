using ControleCerto.Decorators;
using ControleCerto.DTOs.Investment;
using ControleCerto.Extensions;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ExtractTokenInfo]
    public class InvestmentController : ControllerBase
    {
        private readonly IInvestmentService _investmentService;

        public InvestmentController(IInvestmentService investmentService)
        {
            _investmentService = investmentService;
        }

        [HttpPost("CreateInvestment")]
        public async Task<IActionResult> CreateInvestment([FromBody] CreateInvestmentRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.CreateInvestmentAsync(request, userId);
            if (result.IsSuccess) return Created("GetInvestments", result.Value);
            return result.HandleReturnResult();
        }

        [HttpPatch("UpdateInvestment")]
        public async Task<IActionResult> UpdateInvestment([FromBody] UpdateInvestmentRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.UpdateInvestmentAsync(request, userId);
            return result.HandleReturnResult();
        }

        [HttpPost("Deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositInvestmentRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.DepositAsync(request, userId);
            if (result.IsSuccess) return Created("GetInvestments", result.Value);
            return result.HandleReturnResult();
        }

        [HttpPost("Withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] DepositInvestmentRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.WithdrawAsync(request, userId);
            if (result.IsSuccess) return Created("GetInvestments", result.Value);
            return result.HandleReturnResult();
        }

        [HttpPost("AdjustValue")]
        public async Task<IActionResult> AdjustValue([FromBody] AdjustInvestmentRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.AdjustInvestmentAsync(request, userId);
            if (result.IsSuccess) return Created("GetInvestments", result.Value);
            return result.HandleReturnResult();
        }

        [HttpDelete("DeleteInvestment/{investmentId}")]
        public async Task<IActionResult> DeleteInvestment([FromRoute] long investmentId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.DeleteInvestmentAsync(investmentId, userId);
            return result.HandleReturnResult();
        }

        [HttpGet("GetInvestments")]
        public async Task<IActionResult> GetInvestments()
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.GetInvestmentsAsync(userId);
            return result.HandleReturnResult();
        }

        [HttpGet("GetInvestment/{investmentId}")]
        public async Task<IActionResult> GetInvestment([FromRoute] long investmentId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.GetInvestmentAsync(investmentId, userId);
            return result.HandleReturnResult();
        }

        [HttpGet("GetInvestmentHistory/{investmentId}")]
        public async Task<IActionResult> GetInvestmentHistory([FromRoute] long investmentId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _investmentService.GetInvestmentHistoryAsync(investmentId, userId);
            return result.HandleReturnResult();
        }
    }
}
