using Finantech.Decorators;
using Finantech.DTOs.Transaction;
using Finantech.DTOs.TransferenceDTO;
using Finantech.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

            try
            {
                var expense = await _transactionService.CreateTransactionAsync(request, userId);

                return Created("", expense);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteTransaction/{transactionId}")]
        public async Task<IActionResult> DeleteTransaction([FromRoute] int transactionId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                await _transactionService.DeleteTransactionAsync(transactionId, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("UpdateTransaction")]
        public async Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var updatedExpense = await _transactionService.UpdateTransactionAsync(request, userId);

                return Ok(updatedExpense);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateTransference")]
        public async Task<IActionResult> CreateTransference([FromBody] CreateTransferenceRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var transference = await _transactionService.CreateTransferenceAsync(request, userId);

                return Created("", transference);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("GetTransactions")]
        public async Task<IActionResult> GetTransactions
        (
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? accountId
        )
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var transactions = await _transactionService.GetTransactionsAsync(userId, startDate, endDate, accountId);

                return Ok(transactions);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
