using Finantech.Decorators;
using Finantech.DTOs.Account;
using Finantech.Models.DTOs;
using Finantech.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService) 
        { 
            _accountService = accountService; 
        }

        [HttpPut]
        [ExtractTokenInfo]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest accountRequest)
        {
            int userId = (int)HttpContext.Items["UserId"];

            try
            {
                var account = await _accountService.CreateAccountAsync(accountRequest, userId);

                return Created("", account);
            } catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("GetMonthTransactions/{accountId?}")]
        public async Task<IActionResult> GetMonthTransactions(int? accountId) 
        {
            int userId = 1;//pegar da requisicao

            try
            {
                var monthTransactions = await _accountService.GetMonthTransactionsAsync(userId, accountId);

                return Ok(monthTransactions);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        /* public Task<ICollection<InfoTransactionResponse>> GetTransactionsWithPagination(int pageNumber, int pageSize, int userId, int? accountId);
         public Task<InfoAccountResponse> UpdateAccount(UpdateAccountRequest request);
         public Task<ICollection<InfoAccountResponse>> GetAccountsByUserIdAsync(int userId);*/
    }
}
