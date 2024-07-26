using Finantech.Decorators;
using Finantech.DTOs.Account;
using Finantech.Errors;
using Finantech.Extensions;
using Finantech.Models.DTOs;
using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ExtractTokenInfo]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService) 
        { 
            _accountService = accountService; 
        }

        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest accountRequest)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _accountService.CreateAccountAsync(accountRequest, userId);

            if (result.IsSuccess)
            {
                return Created($"Account/{userId}",result.Value);
            }
            else
            {
                return result.HandleReturnResult();
            }

        }

        
        [HttpDelete("DeleteAccount/{accountId}")]
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

        [HttpGet("GetAccountsWithoutCreditCard")]
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

        [HttpPatch("UpdateAccount")]
        public async Task<IActionResult> UpdateAccount(UpdateAccountRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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


        [HttpGet("GetBalanceStatementAsync")]
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

        [HttpGet("GetAccountBalance")]
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
