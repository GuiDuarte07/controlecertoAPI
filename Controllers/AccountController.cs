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
    public class AccountController : Controller
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

        [HttpGet("GetMonthTransactions/{accountId?}")]
        public async Task<IActionResult> GetMonthTransactions(int? accountId) 
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var monthTransactions = await _accountService.GetMonthTransactionsAsync(userId, accountId);

                return Ok(monthTransactions);

            } catch (Exception ex)
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

        /* public Task<ICollection<InfoTransactionResponse>> GetTransactionsWithPagination(int pageNumber, int pageSize, int userId, int? accountId);*/
         
         
    }
}
