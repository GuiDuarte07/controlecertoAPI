using Finantech.Decorators;
using Finantech.DTOs.Transaction;
using Finantech.DTOs.TransferenceDTO;
using Finantech.Errors;
using Finantech.Extensions;
using Finantech.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ExtractTokenInfo]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        public TransactionController(ITransactionService transactionService) 
        {
            _transactionService = transactionService;
        }

        [HttpPost("CreateTransaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _transactionService.CreateTransactionAsync(request, userId);

            if(result.IsSuccess)
            {
                return Created("GetTransactions", result.Value);
            }

            return result.HandleReturnResult();
        }

        [HttpDelete("DeleteTransaction/{transactionId}")]
        public async Task<IActionResult> DeleteTransaction([FromRoute] int transactionId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _transactionService.DeleteTransactionAsync(transactionId, userId);
            return result.HandleReturnResult();
        }

        [HttpPatch("UpdateTransaction")]
        public async Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _transactionService.UpdateTransactionAsync(request, userId);
            return result.HandleReturnResult();
        }

        [HttpPost("CreateTransference")]
        public async Task<IActionResult> CreateTransference([FromBody] CreateTransferenceRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _transactionService.CreateTransferenceAsync(request, userId);

            if (result.IsSuccess)
            {
                return Created("GetTransactions", result.Value);
            }

            return result.HandleReturnResult();
        }

        [HttpGet("GetTransactions")]
        public async Task<IActionResult> GetTransactions
        (
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? accountId
        )
        {
            DateTime utcStartDate = startDate.ToUniversalTime();
            DateTime utcEndDate = endDate.ToUniversalTime();

            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _transactionService.GetTransactionsAsync(userId, utcStartDate, utcEndDate, accountId);

            return result.HandleReturnResult();
        }
    }
}
