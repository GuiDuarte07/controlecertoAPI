using Microsoft.AspNetCore.Http;

namespace ControleCerto.Modules.Tickets.DTOs
{
    public class CreateTicketMessageRequest
    {
        public string Body { get; set; }
        public IFormFile[]? Attachments { get; set; }
    }
}

