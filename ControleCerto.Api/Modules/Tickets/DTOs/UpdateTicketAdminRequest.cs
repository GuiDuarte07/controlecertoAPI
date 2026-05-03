using ControleCerto.Enums;

namespace ControleCerto.Modules.Tickets.DTOs
{
    public class UpdateTicketAdminRequest
    {
        public TicketStatusEnum? Status { get; set; }
        public TicketPriorityEnum? Priority { get; set; }
    }
}

