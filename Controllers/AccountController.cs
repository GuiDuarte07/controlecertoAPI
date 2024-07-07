using Finantech.Decorators;
using Finantech.DTOs.Account;
using Finantech.Models.DTOs;
using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

            try
            {
                var account = await _accountService.CreateAccountAsync(accountRequest, userId);

                return Created("", account);
            } catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }

        }

        
        [HttpDelete("DeleteAccount/{accountId}")]
        public async Task<IActionResult> DeleteAccount([FromRoute] int accountId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                await _accountService.DeleteAccountAsync(accountId, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAccountsByUserIdAsync()
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var accounts = await _accountService.GetAccountsByUserIdAsync(userId);

                return Ok(accounts);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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

            try
            {
                var updatedAccount = await _accountService.UpdateAccountAsync(request, userId);

                return Ok(updatedAccount);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("GetBalanceStatementAsync")]
        public async Task<IActionResult> GetBalanceStatementAsync()
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var balance = await _accountService.GetBalanceStatementAsync(userId, null, null);

                return Ok(balance);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAccountBalance")]
        public async Task<IActionResult> GetAccountBalanceAsync()
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var balance = await _accountService.GetAccountBalanceAsync(userId);

                return Ok(balance);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
