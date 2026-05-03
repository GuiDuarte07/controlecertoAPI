using ControleCerto.Decorators;
using ControleCerto.Extensions;
using ControleCerto.Modules.Tickets.DTOs;
using ControleCerto.Modules.Tickets.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Modules.Tickets.Controllers
{
    [ApiController]
    [Route("api/admin/tickets")]
    [ExtractTokenInfo]
    public class AdminTicketsController : ControllerBase
    {
        private readonly ITicketsService _ticketsService;

        public AdminTicketsController(ITicketsService ticketsService)
        {
            _ticketsService = ticketsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var adminId = (int)HttpContext.Items["UserId"]!;
            var result = await _ticketsService.GetAdminTicketsAsync(adminId, page, pageSize, search, status);
            return result.HandleReturnResult();
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetTicketDetail(long id)
        {
            var adminId = (int)HttpContext.Items["UserId"]!;
            var result = await _ticketsService.GetAdminTicketDetailAsync(id, adminId);
            return result.HandleReturnResult();
        }

        [HttpPost("{id:long}/messages")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateMessage(long id, [FromForm] CreateTicketMessageRequest request)
        {
            var adminId = (int)HttpContext.Items["UserId"]!;
            var result = await _ticketsService.CreateAdminMessageAsync(id, request, adminId);
            if (result.IsSuccess)
            {
                return Created($"/api/admin/tickets/{id}", result.Value);
            }
            return result.HandleReturnResult();
        }

        [HttpPatch("{id:long}")]
        public async Task<IActionResult> UpdateTicket(long id, [FromBody] UpdateTicketAdminRequest request)
        {
            var adminId = (int)HttpContext.Items["UserId"]!;
            var result = await _ticketsService.UpdateAdminTicketAsync(id, request, adminId);
            return result.HandleReturnResult();
        }
    }
}

