using ControleCerto.Decorators;
using ControleCerto.DTOs.CreditCard;
using ControleCerto.DTOs.CreditPurchase;
using ControleCerto.DTOs.Invoice;
using ControleCerto.Errors;
using ControleCerto.Extensions;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/credit-cards")]
    [Authorize]
    [ExtractTokenInfo]
    public class CreditCardController : ControllerBase
    {
        private readonly ICreditCardService _creditCardService;
        public CreditCardController(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCreditCard([FromBody] CreateCreditCardRequest request)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.CreateCreditCardAsync(request, userId);

            if (result.IsSuccess)
            {
                return Created($"/api/credit-cards/{result.Value.Id}", result.Value);
            }

            return result.HandleReturnResult();
        }

        [HttpPatch("{creditCardId:long}")]
        public async Task<IActionResult> UpdateCreditCard([FromRoute] long creditCardId, [FromBody] UpdateCreditCardRequest request)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            request.Id = (int)creditCardId;
            ModelState.Clear();
            TryValidateModel(request);

            if (!ModelState.IsValid)
            {
                var errorResponse = ErrorResponse.FromModelState(ModelState);
                return StatusCode(errorResponse.Code, errorResponse);
            }

            var result = await _creditCardService.UpdateCreditCardAsync(request, userId);

            return result.HandleReturnResult();
        }

        [HttpPost("purchases")]
        public async Task<IActionResult> CreateCreditPurchase([FromBody] CreateCreditPurchaseRequest request)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.CreateCreditPurchaseAsync(request, userId);

            if (result.IsSuccess)
            {
                return Created($"/api/credit-cards/purchases/{result.Value.Id}", result.Value);
            }

            return result.HandleReturnResult();
        }

        [HttpGet("purchases/simulate-invoice")]
        public async Task<IActionResult> SimulateCreditPurchaseInvoice([FromQuery] SimulateCreditPurchaseInvoiceRequest request)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.SimulateCreditPurchaseInvoiceAsync(request, userId);

            return result.HandleReturnResult();
        }

        [HttpPatch("purchases/{creditPurchaseId:long}")]
        public async Task<IActionResult> UpdateCreditPurchase([FromRoute] long creditPurchaseId, [FromBody] UpdateCreditPurchaseRequest request)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            request.Id = creditPurchaseId;
            ModelState.Clear();
            TryValidateModel(request);

            if (!ModelState.IsValid)
            {
                var errorResponse = ErrorResponse.FromModelState(ModelState);
                return StatusCode(errorResponse.Code, errorResponse);
            }

            var result = await _creditCardService.UpdateCreditPurchaseAsync(request, userId);

            return result.HandleReturnResult();
        }
        
        [HttpGet("invoices")]
        public async Task<IActionResult> GetInvoicesByDate
        (
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] long? creditCardId
        )
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var startDateSet = startDate ?? DateTime.MinValue;
            var endDateSet = endDate ?? DateTime.MaxValue;
            
            startDateSet = new DateTime(startDateSet.Year, startDateSet.Month, 1, 0, 0, 0);
            endDateSet = new DateTime(endDateSet.Year, endDateSet.Month, 1, 0, 0, 0);

            var result = await _creditCardService.GetInvoicesByDateAsync(userId, startDateSet, endDateSet, creditCardId);

            return result.HandleReturnResult();
        }

        
        [HttpGet("invoices/{invoiceId:long}")]
        public async Task<IActionResult> GetInvoicesById
        (
            long invoiceId
        )
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.GetInvoicesByIdAsync(invoiceId, userId);

            return result.HandleReturnResult();
        }

        [HttpGet("invoices/{invoiceId:int}/expenses")]
        public async Task<IActionResult> GetCreditExpensesFromInvoice(int invoiceId)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.GetCreditExpensesFromInvoice(invoiceId, userId);

            return result.HandleReturnResult();
        }

        [HttpPost("invoices/{invoiceId:long}/payments")]
        public async Task<IActionResult> PayInvoice([FromRoute] long invoiceId, [FromBody] CreteInvoicePaymentRequest request)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            request.InvoiceId = invoiceId;
            ModelState.Clear();
            TryValidateModel(request);

            if (!ModelState.IsValid)
            {
                var errorResponse = ErrorResponse.FromModelState(ModelState);
                return StatusCode(errorResponse.Code, errorResponse);
            }

            var result = await _creditCardService.PayInvoiceAsync(request, userId);

            if (result.IsSuccess)
            {
                return Created($"/api/credit-cards/invoices/{invoiceId}/payments/{result.Value.Id}", result.Value);
            }

            return result.HandleReturnResult();

        }

        [HttpDelete("payments/{id:long}")]
        public async Task<IActionResult> DeleteInvoicePayment(
            long id)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.DeleteInvoicePaymentAsync(id, userId);

            return result.HandleReturnResult();
        }

        [HttpDelete("purchases/{creditPurchaseId:long}")]
        public async Task<IActionResult> DeleteCreditPurchase([FromRoute] long creditPurchaseId)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

           
            var result = await _creditCardService.DeleteCreditPurchaseAsync(creditPurchaseId, userId);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            return result.HandleReturnResult();

        }

        [HttpGet("info")]
        public async Task<IActionResult> GetCreditCardInfo()
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _creditCardService.GetCreditCardInfo(userId);

            return result.HandleReturnResult();

        }
    }
}
