using Microsoft.AspNetCore.Http;

namespace ControleCerto.Modules.Tickets.DTOs
{
    public class CreateTicketRequest
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public IFormFile[]? Attachments { get; set; }
    }
}

