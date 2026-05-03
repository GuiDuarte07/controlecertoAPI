using ControleCerto.Decorators;
using ControleCerto.Extensions;
using ControleCerto.Modules.Tickets.DTOs;
using ControleCerto.Modules.Tickets.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Modules.Tickets.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    [ExtractTokenInfo]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketsService _ticketsService;

        public TicketsController(ITicketsService ticketsService)
        {
            _ticketsService = ticketsService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateTicket([FromForm] CreateTicketRequest request)
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            var result = await _ticketsService.CreateTicketAsync(request, userId);
            if (result.IsSuccess)
            {
                return Created($"/api/tickets/{result.Value.Id}", result.Value);
            }
            return result.HandleReturnResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            var result = await _ticketsService.GetUserTicketsAsync(userId, page, pageSize, search, status);
            return result.HandleReturnResult();
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetTicketDetail(long id)
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            var result = await _ticketsService.GetTicketDetailAsync(id, userId);
            return result.HandleReturnResult();
        }

        [HttpPost("{id:long}/messages")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateMessage(long id, [FromForm] CreateTicketMessageRequest request)
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            var result = await _ticketsService.CreateUserMessageAsync(id, request, userId);
            if (result.IsSuccess)
            {
                return Created($"/api/tickets/{id}", result.Value);
            }
            return result.HandleReturnResult();
        }

        [HttpPatch("{id:long}")]
        public async Task<IActionResult> UpdateTicket(long id, [FromBody] UpdateTicketUserRequest request)
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            var result = await _ticketsService.UpdateUserTicketAsync(id, request, userId);
            return result.HandleReturnResult();
        }
    }
}

