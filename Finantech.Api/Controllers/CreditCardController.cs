using Finantech.Decorators;
using Finantech.DTOs.CreditCard;
using Finantech.DTOs.CreditPurchase;
using Finantech.DTOs.Invoice;
using Finantech.Errors;
using Finantech.Extensions;
using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
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

            var result = await _creditCardService.CreateCreditCardAsync(request, userId);

            if (result.IsSuccess)
            {
                return Created("", result.Value);
            }

            return result.HandleReturnResult();
        }

        [HttpPatch("UpdateCreditCard")]
        public async Task<IActionResult> UpdateCreditCard([FromBody] UpdateCreditCardRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.UpdateCreditCardAsync(request, userId);

            return result.HandleReturnResult();
        }

        [HttpPost("CreateCreditPurchase")]
        public async Task<IActionResult> CreateCreditPurchase([FromBody] CreateCreditPurchaseRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.CreateCreditPurchaseAsync(request, userId);

            if (result.IsSuccess)
            {
                return Created("", result.Value);
            }

            return result.HandleReturnResult();
        }

        [HttpPatch("UpdateCreditPurchase")]
        public async Task<IActionResult> UpdateCreditPurchase([FromBody] UpdateCreditPurchaseRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.UpdateCreditPurchaseAsync(request, userId);

            return result.HandleReturnResult();
        }
        
        [HttpGet("GetInvoicesByDate")]
        public async Task<IActionResult> GetInvoicesByDate
        (
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] long? creditCardId
        )
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            DateTime startDateSet = startDate ?? DateTime.MinValue;
            DateTime endDateSet = endDate ?? DateTime.MaxValue;

            var result = await _creditCardService.GetInvoicesByDateAsync(userId, startDateSet, endDateSet, creditCardId);

            return result.HandleReturnResult();
        }

        
        [HttpGet("GetInvoicesById/{invoiceId}")]
        public async Task<IActionResult> GetInvoicesById
        (
            long invoiceId
        )
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.GetInvoicesByIdAsync(invoiceId, userId);

            return result.HandleReturnResult();
        }

        [HttpGet("GetCreditExpensesFromInvoice/{invoiceId}")]
        public async Task<IActionResult> GetCreditExpensesFromInvoice(int invoiceId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.GetCreditExpensesFromInvoice(invoiceId, userId);

            return result.HandleReturnResult();
        }

        [HttpPost("PayInvoice")]
        public async Task<IActionResult> PayInvoice([FromBody] CreteInvoicePaymentRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.PayInvoiceAsync(request, userId);

            if (result.IsSuccess)
            {
                return Created("", result.Value);
            }

            return result.HandleReturnResult();

        }

        [HttpDelete("DeleteCreditPurchase/{creditPurchaseId}")]
        public async Task<IActionResult> DeleteCreditPurchase([FromRoute] int creditPurchaseId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

           
            var result = await _creditCardService.DeleteCreditPurchaseAsync(creditPurchaseId, userId);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            return result.HandleReturnResult();

        }

        [HttpGet("GetCreditCardInfo")]
        public async Task<IActionResult> GetCreditCardInfo()
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.GetCreditCardInfo(userId);

            return result.HandleReturnResult();

        }
    }
}
