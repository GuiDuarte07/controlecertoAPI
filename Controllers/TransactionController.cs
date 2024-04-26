using Finantech.Decorators;
using Finantech.DTOs.Expense;
using Finantech.DTOs.Income;
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

        [HttpPost("CreateExpense")]
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var expense = await _transactionService.CreateExpenseAsync(request, userId);

                return Created("", expense);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpDelete("DeleteExpense/{expenseId}")]
        public async Task<IActionResult> DeleteExpense([FromRoute] int expenseId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                await _transactionService.DeleteExpenseAsync(expenseId, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
