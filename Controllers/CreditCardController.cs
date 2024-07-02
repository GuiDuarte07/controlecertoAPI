using Finantech.Decorators;
using Finantech.DTOs.CreditCard;
using Finantech.DTOs.CreditPurcchase;
using Finantech.DTOs.Invoice;
using Finantech.Models.Entities;
using Finantech.Services;
using Finantech.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ExtractTokenInfo]
    public class CreditCardController : ControllerBase
    {
        private readonly ICreditCardService _creditCardService;
        public CreditCardController(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        [HttpPost("CreateCreditCard")]
        public async Task<IActionResult> CreateCreditCard([FromBody] CreateCreditCardRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var createdCreditCard = await _creditCardService.CreateCreditCardAsync(request, userId);

                return Created("", createdCreditCard);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("UpdateCreditCard")]
        public async Task<IActionResult> UpdateCreditCard([FromBody] UpdateCreditCardRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var updatedCreditCard = await _creditCardService.UpdateCreditCardAsync(request, userId);

                return Created("", updatedCreditCard);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateCreditPurchase")]
        public async Task<IActionResult> CreateCreditPurchase([FromBody] CreateCreditPurchaseRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var updatedCreditCard = await _creditCardService.CreateCreditPurchaseAsync(request, userId);

                return Created("", updatedCreditCard);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetInvoicesWithPagination")]
        public async Task<IActionResult> GetInvoicesWithPagination
        (
            [FromQuery] int pageNumber,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? accountId
        )
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            const int pageSize = 12;

            DateTime startDateSet = startDate ?? DateTime.MinValue;
            DateTime endDateSet = endDate ?? DateTime.MaxValue;

            if (pageNumber < 1) pageNumber = 1;

            try
            {
                var invoices = await _creditCardService.GetInvoicesWithPaginationAsync(pageNumber, pageSize, userId, startDateSet, endDateSet, accountId);

                return Ok(invoices);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetCreditExpensesFromInvoice/{invoiceId}")]
        public async Task<IActionResult> GetCreditExpensesFromInvoice(int invoiceId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            try
            {
                var creditExpenses = await _creditCardService.GetCreditExpensesFromInvoice(invoiceId, userId);

                return Ok(creditExpenses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("PayInvoice")]
        public async Task<IActionResult> PayInvoice([FromBody] CreteInvoicePaymentRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var invoicePayment = await _creditCardService.PayInvoiceAsync(request, userId);

                return Created("", invoicePayment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteCreditPurchase/{creditPurchaseId}")]
        public async Task<IActionResult> DeleteCreditPurchase([FromRoute] int creditPurchaseId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                await _creditCardService.DeleteCreditPurchaseAsync(creditPurchaseId, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetCreditCardInfo")]
        public async Task<IActionResult> GetCreditCardInfo()
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            try
            {
                var creditCards = await _creditCardService.GetCreditCardInfo(userId);

                return Ok(creditCards);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
