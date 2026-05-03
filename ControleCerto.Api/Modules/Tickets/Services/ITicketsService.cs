using ControleCerto.DTOs.Common;
using ControleCerto.Errors;
using ControleCerto.Modules.Tickets.DTOs;

namespace ControleCerto.Modules.Tickets.Services
{
    public interface ITicketsService
    {
        Task<Result<TicketDetailResponse>> CreateTicketAsync(CreateTicketRequest request, int userId);
        Task<Result<PaginatedResponse<TicketResponse>>> GetUserTicketsAsync(int userId, int page, int pageSize, string? search, string? status);
        Task<Result<TicketDetailResponse>> GetTicketDetailAsync(long ticketId, int userId);
        Task<Result<TicketMessageResponse>> CreateUserMessageAsync(long ticketId, CreateTicketMessageRequest request, int userId);
        Task<Result<bool>> UpdateUserTicketAsync(long ticketId, UpdateTicketUserRequest request, int userId);

        Task<Result<PaginatedResponse<TicketResponse>>> GetAdminTicketsAsync(int adminId, int page, int pageSize, string? search, string? status);
        Task<Result<TicketDetailResponse>> GetAdminTicketDetailAsync(long ticketId, int adminId);
        Task<Result<TicketMessageResponse>> CreateAdminMessageAsync(long ticketId, CreateTicketMessageRequest request, int adminId);
        Task<Result<TicketResponse>> UpdateAdminTicketAsync(long ticketId, UpdateTicketAdminRequest request, int adminId);
    }
}

