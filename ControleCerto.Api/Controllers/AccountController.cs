using ControleCerto.Decorators;
using ControleCerto.DTOs.Account;
using ControleCerto.Errors;
using ControleCerto.Extensions;
using ControleCerto.Models.DTOs;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Authorize]
    [ExtractTokenInfo]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService) 
        { 
            _accountService = accountService; 
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest accountRequest)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _accountService.CreateAccountAsync(accountRequest, userId);

            if (result.IsSuccess)
            {
                return Created($"/api/accounts/{result.Value.Id}", result.Value);
            }
            else
            {
                return result.HandleReturnResult();
            }

        }

        
        [HttpDelete("{accountId:int}")]
        public async Task<IActionResult> DeleteAccount([FromRoute] int accountId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;
            var result = await _accountService.DeleteAccountAsync(accountId, userId);

            if (result.IsSuccess)
            {
                return NoContent();
            }
            else
            {
                return result.HandleReturnResult();
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAccountsByUserIdAsync()
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _accountService.GetAccountsByUserIdAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            } else
            {
                return result.HandleReturnResult();
            }
        }

        [HttpGet("without-credit-card")]
        public async Task<IActionResult> GetAccountsWithoutCreditCard()
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _accountService.GetAccountsWithoutCreditCardAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else
            {
                return result.HandleReturnResult();
            }
        }

        [HttpPatch("{accountId:int}")]
        public async Task<IActionResult> UpdateAccount([FromRoute] int accountId, [FromBody] UpdateAccountRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            request.Id = accountId;

            ModelState.Clear();
            TryValidateModel(request);

            if (!ModelState.IsValid)
            {
                var errorResponse = ErrorResponse.FromModelState(ModelState);
                return StatusCode(errorResponse.Code, errorResponse);
            }

            var result = await _accountService.UpdateAccountAsync(request, userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else
            {
                return result.HandleReturnResult();
            }
        }


        [HttpGet("balance-statement")]
        public async Task<IActionResult> GetBalanceStatementAsync()
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _accountService.GetBalanceStatementAsync(userId, null, null);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else
            {
                return result.HandleReturnResult();
            }
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetAccountBalanceAsync()
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _accountService.GetAccountBalanceAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else
            {
                return result.HandleReturnResult();
            }
        }
    }
}
