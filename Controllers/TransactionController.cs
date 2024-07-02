using Finantech.Decorators;
using Finantech.DTOs.Account;
using Finantech.DTOs.Expense;
using Finantech.DTOs.Income;
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

        [HttpPatch("UpdateExpense")]
        public async Task<IActionResult> UpdateExpense([FromBody] UpdateExpenseRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var updatedExpense = await _transactionService.UpdateExpenseAsync(request, userId);

                return Ok(updatedExpense);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateIncome")]
        public async Task<IActionResult> CreateIncome([FromBody] CreateIncomeRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var income = await _transactionService.CreateIncomeAsync(request, userId);

                return Created("", income);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpDelete("DeleteIncome/{incomeId}")]
        public async Task<IActionResult> DeleteIncome([FromRoute] int incomeId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                await _transactionService.DeleteIncomeAsync(incomeId, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("UpdateIncome")]
        public async Task<IActionResult> UpdateIncome([FromBody] UpdateIncomeRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var updatedIncome = await _transactionService.UpdateIncomeAsync(request, userId);

                return Ok(updatedIncome);
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

        [HttpGet("GetTransactionsWithPagination")]
        public async Task<IActionResult> GetTransactionsWithPagination
        (
            [FromQuery]int pageNumber,
            [FromQuery] int? accountId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate
        )
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            const int pageSize = 30;

            DateTime startDateSet = startDate ?? DateTime.MinValue;
            DateTime endDateSet = endDate ?? DateTime.Now;

            if (pageNumber < 1) pageNumber = 1;

            try
            {
                var transactions = await _transactionService.GetTransactionsWithPaginationAsync(pageNumber, pageSize, userId, startDateSet, endDateSet, accountId);

                return Ok(transactions);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
