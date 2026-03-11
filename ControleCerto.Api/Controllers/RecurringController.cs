using ControleCerto.Decorators;
using ControleCerto.DTOs.RecurringTransaction;
using ControleCerto.DTOs.Transaction;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Extensions;
using ControleCerto.Services;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/recurring-transactions")]
    [Authorize]
    [ExtractTokenInfo]
    public class RecurringController : ControllerBase
    {
        private readonly IRecurringTransactionService _recurringService;
        public RecurringController(IRecurringTransactionService recurringService)
        {
            _recurringService = recurringService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecurring([FromBody] CreateRecurringTransactionRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _recurringService.CreateReccuringTransactionAsync(request, userId);

            if (result.IsSuccess)
            {
                return Created($"/api/recurring-transactions/{result.Value.Id}", result.Value);
            }

            return result.HandleReturnResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetRecurringTransactions()
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _recurringService.GetRecurringTransactionsAsync(userId);

            return Ok(result);
        }

        [HttpPatch("{recurringId:long}")]
        public async Task<IActionResult> UpdateRecurringTransaction([FromRoute] long recurringId, [FromBody] UpdateRecurringTransactionRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _recurringService.UpdateRecurringTransactionAsync(recurringId, request, userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return result.HandleReturnResult();
        }

        [HttpDelete("{recurringId:long}")]
        public async Task<IActionResult> DeleteRecurringTransaction([FromRoute] long recurringId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _recurringService.DeleteRecurringTransactionAsync(recurringId, userId);

            return result.HandleReturnResult();
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPendingRecurringTransactionInstances([FromBody] ProcessPendingRecurringRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _recurringService.ProcessPendingRecurringTransactionInstances(request.PendingTransactions, request.Action, userId, request.RejectReason);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return result.HandleReturnResult();
        }

        [HttpGet("instances")]
        public async Task<IActionResult> GetRecurringTransactionInstancesAsync([FromQuery] InstanceStatusEnum status, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            if (!Enum.IsDefined(typeof(InstanceStatusEnum), status))
            {
                var errorResponse = ErrorResponse.FromAppError(
                    new AppError("O status informado não é válido.", ErrorTypeEnum.Validation),
                    StatusCodes.Status400BadRequest);

                return StatusCode(errorResponse.Code, errorResponse);
            }

            var result = await _recurringService.GetRecurringTransactionInstancesAsync(status, userId, startDate, endDate);

            return Ok(result);
        }

    }
}
