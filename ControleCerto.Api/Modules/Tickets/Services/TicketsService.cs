using ControleCerto.DTOs.Common;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Modules.Tickets.DTOs;
using ControleCerto.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ControleCerto.Modules.Tickets.Services
{
    public class TicketsService : ITicketsService
    {
        private const int MaxAttachmentsPerRequest = 10;
        private const long MaxAttachmentSizeBytes = 10 * 1024 * 1024;
        private readonly AppDbContext _appDbContext;
        private readonly IS3Service _s3Service;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public TicketsService(AppDbContext appDbContext, IS3Service s3Service, IEmailService emailService, IConfiguration configuration)
        {
            _appDbContext = appDbContext;
            _s3Service = s3Service;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<Result<TicketDetailResponse>> CreateTicketAsync(CreateTicketRequest request, int userId)
        {
            var user = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                return new AppError("Usuário não encontrado.", ErrorTypeEnum.NotFound);
            }

            var now = DateTime.UtcNow;
            var ticket = new Ticket
            {
                UserId = userId,
                Subject = request.Subject.Trim(),
                Status = TicketStatusEnum.OPEN,
                Priority = TicketPriorityEnum.NORMAL,
                CreatedAt = now,
                UpdatedAt = now,
            };

            await _appDbContext.Tickets.AddAsync(ticket);
            await _appDbContext.SaveChangesAsync();

            var message = new TicketMessage
            {
                TicketId = ticket.Id,
                AuthorUserId = userId,
                AuthorRole = TicketMessageAuthorRoleEnum.USER,
                Body = request.Description?.Trim() ?? string.Empty,
                CreatedAt = now,
            };

            await _appDbContext.TicketMessages.AddAsync(message);
            await _appDbContext.SaveChangesAsync();

            try
            {
                var attachments = await UploadAttachmentsAsync(ticket.Id, message.Id, userId, request.Attachments);
                if (attachments.Count > 0)
                {
                    await _appDbContext.TicketAttachments.AddRangeAsync(attachments);
                    await _appDbContext.SaveChangesAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                return new AppError(ex.Message, ErrorTypeEnum.Validation);
            }

            await NotifyAdminsAsync(
                title: "Novo chamado",
                message: $"{user.Name} abriu um chamado: {ticket.Subject}",
                actionPath: $"/admin/tickets/{ticket.Id}",
                emailSubject: $"[ControleCerto] Novo chamado: {ticket.Subject}",
                emailBodyHtml: BuildEmailBody(
                    heading: "Novo chamado",
                    paragraph: $"{user.Name} abriu um chamado: <strong>{EscapeHtml(ticket.Subject)}</strong>",
                    actionUrl: BuildFrontendUrl($"/pt/admin/tickets/{ticket.Id}"),
                    actionText: "Abrir chamado"
                )
            );

            return await GetTicketDetailInternalAsync(ticket.Id, userId, isAdmin: false);
        }

        public async Task<Result<PaginatedResponse<TicketResponse>>> GetUserTicketsAsync(int userId, int page, int pageSize, string? search, string? status)
        {
            var query = _appDbContext.Tickets.AsNoTracking().Where(t => t.UserId == userId);
            query = ApplyFilters(query, search, status);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PaginatedResponse<TicketResponse>
            {
                Data = items.Select(MapTicket).ToList(),
                Pagination = new PaginationMetadata(page, pageSize, totalItems)
            };

            return response;
        }

        public async Task<Result<TicketDetailResponse>> GetTicketDetailAsync(long ticketId, int userId)
        {
            return await GetTicketDetailInternalAsync(ticketId, userId, isAdmin: false);
        }

        public async Task<Result<TicketMessageResponse>> CreateUserMessageAsync(long ticketId, CreateTicketMessageRequest request, int userId)
        {
            var ticket = await _appDbContext.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.UserId == userId);
            if (ticket is null)
            {
                return new AppError("Chamado não encontrado.", ErrorTypeEnum.NotFound);
            }

            if (ticket.Status == TicketStatusEnum.CLOSED)
            {
                return new AppError("Chamado encerrado não aceita novas mensagens.", ErrorTypeEnum.BusinessRule);
            }

            var now = DateTime.UtcNow;
            var message = new TicketMessage
            {
                TicketId = ticket.Id,
                AuthorUserId = userId,
                AuthorRole = TicketMessageAuthorRoleEnum.USER,
                Body = request.Body?.Trim() ?? string.Empty,
                CreatedAt = now,
            };

            await _appDbContext.TicketMessages.AddAsync(message);

            ticket.UpdatedAt = now;
            if (ticket.Status == TicketStatusEnum.WAITINGUSER)
            {
                ticket.Status = TicketStatusEnum.INPROGRESS;
            }

            await _appDbContext.SaveChangesAsync();

            try
            {
                var attachments = await UploadAttachmentsAsync(ticket.Id, message.Id, userId, request.Attachments);
                if (attachments.Count > 0)
                {
                    await _appDbContext.TicketAttachments.AddRangeAsync(attachments);
                    await _appDbContext.SaveChangesAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                return new AppError(ex.Message, ErrorTypeEnum.Validation);
            }

            var user = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user is not null)
            {
                await NotifyAdminsAsync(
                    title: "Nova mensagem no chamado",
                    message: $"{user.Name} enviou uma mensagem no chamado: {ticket.Subject}",
                    actionPath: $"/admin/tickets/{ticket.Id}",
                    emailSubject: $"[ControleCerto] Nova mensagem no chamado: {ticket.Subject}",
                    emailBodyHtml: BuildEmailBody(
                        heading: "Nova mensagem no chamado",
                        paragraph: $"{user.Name} enviou uma nova mensagem no chamado <strong>{EscapeHtml(ticket.Subject)}</strong>.",
                        actionUrl: BuildFrontendUrl($"/pt/admin/tickets/{ticket.Id}"),
                        actionText: "Abrir chamado"
                    )
                );
            }

            var saved = await _appDbContext.TicketMessages
                .AsNoTracking()
                .Include(m => m.Attachments)
                .FirstAsync(m => m.Id == message.Id);

            return MapMessage(saved);
        }

        public async Task<Result<bool>> UpdateUserTicketAsync(long ticketId, UpdateTicketUserRequest request, int userId)
        {
            var ticket = await _appDbContext.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.UserId == userId);
            if (ticket is null)
            {
                return new AppError("Chamado não encontrado.", ErrorTypeEnum.NotFound);
            }

            var action = (request.Action ?? string.Empty).Trim().ToLowerInvariant();
            var now = DateTime.UtcNow;

            if (action == "close")
            {
                if (ticket.Status == TicketStatusEnum.CLOSED)
                {
                    return true;
                }
                ticket.Status = TicketStatusEnum.CLOSED;
                ticket.ClosedAt = now;
                ticket.UpdatedAt = now;

                await _appDbContext.SaveChangesAsync();

                var user = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                await NotifyAdminsAsync(
                    title: "Chamado encerrado pelo usuário",
                    message: $"O usuário encerrou o chamado: {ticket.Subject}",
                    actionPath: $"/admin/tickets/{ticket.Id}",
                    emailSubject: $"[ControleCerto] Chamado encerrado: {ticket.Subject}",
                    emailBodyHtml: BuildEmailBody(
                        heading: "Chamado encerrado",
                        paragraph: $"O chamado <strong>{EscapeHtml(ticket.Subject)}</strong> foi encerrado pelo usuário.",
                        actionUrl: BuildFrontendUrl($"/pt/admin/tickets/{ticket.Id}"),
                        actionText: "Abrir chamado"
                    )
                );

                return true;
            }

            if (action == "reopen")
            {
                if (ticket.Status != TicketStatusEnum.CLOSED)
                {
                    return new AppError("Apenas chamados encerrados podem ser reabertos.", ErrorTypeEnum.BusinessRule);
                }

                ticket.Status = TicketStatusEnum.OPEN;
                ticket.ClosedAt = null;
                ticket.UpdatedAt = now;
                await _appDbContext.SaveChangesAsync();

                await NotifyAdminsAsync(
                    title: "Chamado reaberto pelo usuário",
                    message: $"O usuário reabriu o chamado: {ticket.Subject}",
                    actionPath: $"/admin/tickets/{ticket.Id}",
                    emailSubject: $"[ControleCerto] Chamado reaberto: {ticket.Subject}",
                    emailBodyHtml: BuildEmailBody(
                        heading: "Chamado reaberto",
                        paragraph: $"O chamado <strong>{EscapeHtml(ticket.Subject)}</strong> foi reaberto pelo usuário.",
                        actionUrl: BuildFrontendUrl($"/pt/admin/tickets/{ticket.Id}"),
                        actionText: "Abrir chamado"
                    )
                );

                return true;
            }

            return new AppError("Ação inválida.", ErrorTypeEnum.Validation);
        }

        public async Task<Result<PaginatedResponse<TicketResponse>>> GetAdminTicketsAsync(int adminId, int page, int pageSize, string? search, string? status)
        {
            var admin = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == adminId);
            if (admin is null || !IsAdmin(admin))
            {
                return new AppError("Usuário não autorizado.", ErrorTypeEnum.Validation);
            }

            var query = _appDbContext.Tickets.AsNoTracking();
            query = ApplyFilters(query, search, status);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PaginatedResponse<TicketResponse>
            {
                Data = items.Select(MapTicket).ToList(),
                Pagination = new PaginationMetadata(page, pageSize, totalItems)
            };

            return response;
        }

        public async Task<Result<TicketDetailResponse>> GetAdminTicketDetailAsync(long ticketId, int adminId)
        {
            return await GetTicketDetailInternalAsync(ticketId, adminId, isAdmin: true);
        }

        public async Task<Result<TicketMessageResponse>> CreateAdminMessageAsync(long ticketId, CreateTicketMessageRequest request, int adminId)
        {
            var admin = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == adminId);
            if (admin is null || !IsAdmin(admin))
            {
                return new AppError("Usuário não autorizado.", ErrorTypeEnum.Validation);
            }

            var ticket = await _appDbContext.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket is null)
            {
                return new AppError("Chamado não encontrado.", ErrorTypeEnum.NotFound);
            }

            if (ticket.Status == TicketStatusEnum.CLOSED)
            {
                return new AppError("Chamado encerrado não aceita novas mensagens.", ErrorTypeEnum.BusinessRule);
            }

            var now = DateTime.UtcNow;
            var message = new TicketMessage
            {
                TicketId = ticket.Id,
                AuthorUserId = adminId,
                AuthorRole = TicketMessageAuthorRoleEnum.ADMIN,
                Body = request.Body?.Trim() ?? string.Empty,
                CreatedAt = now,
            };

            await _appDbContext.TicketMessages.AddAsync(message);
            ticket.UpdatedAt = now;
            if (ticket.Status == TicketStatusEnum.OPEN)
            {
                ticket.Status = TicketStatusEnum.INPROGRESS;
            }

            await _appDbContext.SaveChangesAsync();

            try
            {
                var attachments = await UploadAttachmentsAsync(ticket.Id, message.Id, adminId, request.Attachments);
                if (attachments.Count > 0)
                {
                    await _appDbContext.TicketAttachments.AddRangeAsync(attachments);
                    await _appDbContext.SaveChangesAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                return new AppError(ex.Message, ErrorTypeEnum.Validation);
            }

            var targetUser = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == ticket.UserId);
            if (targetUser is not null)
            {
                await NotifyUserAsync(
                    userId: targetUser.Id,
                    userEmail: targetUser.Email,
                    title: "Resposta no seu chamado",
                    message: $"Você recebeu uma resposta no chamado: {ticket.Subject}",
                    actionPath: $"/tickets/{ticket.Id}",
                    emailSubject: $"[ControleCerto] Resposta no chamado: {ticket.Subject}",
                    emailBodyHtml: BuildEmailBody(
                        heading: "Resposta no seu chamado",
                        paragraph: $"Você recebeu uma resposta no chamado <strong>{EscapeHtml(ticket.Subject)}</strong>.",
                        actionUrl: BuildFrontendUrl($"/pt/tickets/{ticket.Id}"),
                        actionText: "Abrir chamado"
                    )
                );
            }

            var saved = await _appDbContext.TicketMessages
                .AsNoTracking()
                .Include(m => m.Attachments)
                .FirstAsync(m => m.Id == message.Id);

            return MapMessage(saved);
        }

        public async Task<Result<TicketResponse>> UpdateAdminTicketAsync(long ticketId, UpdateTicketAdminRequest request, int adminId)
        {
            var admin = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == adminId);
            if (admin is null || !IsAdmin(admin))
            {
                return new AppError("Usuário não autorizado.", ErrorTypeEnum.Validation);
            }

            var ticket = await _appDbContext.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket is null)
            {
                return new AppError("Chamado não encontrado.", ErrorTypeEnum.NotFound);
            }

            var now = DateTime.UtcNow;
            var changed = false;

            if (request.Priority.HasValue && ticket.Priority != request.Priority.Value)
            {
                ticket.Priority = request.Priority.Value;
                changed = true;
            }

            if (request.Status.HasValue && ticket.Status != request.Status.Value)
            {
                ticket.Status = request.Status.Value;
                if (ticket.Status == TicketStatusEnum.CLOSED)
                {
                    ticket.ClosedAt = now;
                }
                if (ticket.Status != TicketStatusEnum.CLOSED)
                {
                    ticket.ClosedAt = null;
                }
                changed = true;
            }

            if (!changed)
            {
                return MapTicket(ticket);
            }

            ticket.UpdatedAt = now;
            await _appDbContext.SaveChangesAsync();

            var targetUser = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == ticket.UserId);
            if (targetUser is not null)
            {
                await NotifyUserAsync(
                    userId: targetUser.Id,
                    userEmail: targetUser.Email,
                    title: "Atualização no seu chamado",
                    message: $"Seu chamado foi atualizado: {ticket.Subject}",
                    actionPath: $"/tickets/{ticket.Id}",
                    emailSubject: $"[ControleCerto] Atualização no chamado: {ticket.Subject}",
                    emailBodyHtml: BuildEmailBody(
                        heading: "Atualização no seu chamado",
                        paragraph: $"Seu chamado <strong>{EscapeHtml(ticket.Subject)}</strong> foi atualizado.",
                        actionUrl: BuildFrontendUrl($"/pt/tickets/{ticket.Id}"),
                        actionText: "Abrir chamado"
                    )
                );
            }

            return MapTicket(ticket);
        }

        private async Task<Result<TicketDetailResponse>> GetTicketDetailInternalAsync(long ticketId, int requesterId, bool isAdmin)
        {
            var requester = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == requesterId);
            if (requester is null)
            {
                return new AppError("Usuário não encontrado.", ErrorTypeEnum.NotFound);
            }

            if (isAdmin && !IsAdmin(requester))
            {
                return new AppError("Usuário não autorizado.", ErrorTypeEnum.Validation);
            }

            var query = _appDbContext.Tickets
                .AsNoTracking()
                .Include(t => t.Messages)
                .ThenInclude(m => m.Attachments)
                .Include(t => t.Attachments);

            var ticket = await query.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket is null)
            {
                return new AppError("Chamado não encontrado.", ErrorTypeEnum.NotFound);
            }

            if (!isAdmin && ticket.UserId != requesterId)
            {
                return new AppError("Chamado não encontrado.", ErrorTypeEnum.NotFound);
            }

            var response = MapTicketDetail(ticket);
            response.Messages = ticket.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(MapMessage)
                .ToList();
            response.Attachments = ticket.Attachments
                .OrderByDescending(a => a.CreatedAt)
                .Select(MapAttachment)
                .ToList();

            return response;
        }

        private IQueryable<Ticket> ApplyFilters(IQueryable<Ticket> query, string? search, string? status)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalized = search.Trim();
                query = query.Where(t => t.Subject.Contains(normalized));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalized = status.Trim().ToUpperInvariant();
                if (Enum.TryParse<TicketStatusEnum>(normalized, out var parsed))
                {
                    query = query.Where(t => t.Status == parsed);
                }
            }

            return query;
        }

        private async Task<List<TicketAttachment>> UploadAttachmentsAsync(long ticketId, long messageId, int userId, IFormFile[]? files)
        {
            if (files is null || files.Length == 0)
            {
                return new List<TicketAttachment>();
            }

            if (files.Length > MaxAttachmentsPerRequest)
            {
                throw new InvalidOperationException("Quantidade de anexos excede o limite.");
            }

            var attachments = new List<TicketAttachment>();

            foreach (var file in files)
            {
                if (file is null) continue;
                if (file.Length <= 0) continue;
                if (file.Length > MaxAttachmentSizeBytes)
                {
                    throw new InvalidOperationException("Arquivo excede o tamanho máximo permitido.");
                }

                var safeName = SanitizeFileName(file.FileName);
                var key = $"tickets/{ticketId}/{messageId}/{Guid.NewGuid():N}_{safeName}";
                var url = await _s3Service.UploadFileAsync(file, key);

                attachments.Add(new TicketAttachment
                {
                    TicketId = ticketId,
                    TicketMessageId = messageId,
                    UploadedByUserId = userId,
                    FileName = file.FileName,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    SizeBytes = file.Length,
                    StorageKey = key,
                    Url = url,
                    CreatedAt = DateTime.UtcNow,
                });
            }

            return attachments;
        }

        private async Task NotifyAdminsAsync(string title, string message, string actionPath, string emailSubject, string emailBodyHtml)
        {
            var adminUsers = await _appDbContext.Users
                .AsNoTracking()
                .Where(u => u.IsAdmin || u.UserType == UserTypeEnum.ADMIN)
                .ToListAsync();
            if (adminUsers.Count == 0)
            {
                return;
            }

            var expiresAt = DateTime.UtcNow.AddDays(7);
            var notifications = adminUsers
                .Select(u => new Notification(title, message, NotificationTypeEnum.TICKETUPDATE, actionPath, expiresAt, u.Id))
                .ToList();

            await _appDbContext.Notifications.AddRangeAsync(notifications);
            await _appDbContext.SaveChangesAsync();

            var emails = adminUsers.Select(u => u.Email).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
            if (emails.Count > 0)
            {
                _emailService.SendEmail(emails, emailSubject, emailBodyHtml);
            }
        }

        private async Task NotifyUserAsync(int userId, string userEmail, string title, string message, string actionPath, string emailSubject, string emailBodyHtml)
        {
            var expiresAt = DateTime.UtcNow.AddDays(7);
            await _appDbContext.Notifications.AddAsync(new Notification(title, message, NotificationTypeEnum.TICKETUPDATE, actionPath, expiresAt, userId));
            await _appDbContext.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                _emailService.SendEmail(new List<string> { userEmail }, emailSubject, emailBodyHtml);
            }
        }

        private bool IsAdmin(User user)
        {
            return user.IsAdmin || user.UserType == UserTypeEnum.ADMIN;
        }

        private string BuildFrontendUrl(string path)
        {
            var host = _configuration.GetConnectionString("WebSiteUrl") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(host)) return path;
            return $"{host.TrimEnd('/')}/{path.TrimStart('/')}";
        }

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "file";
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(fileName.Length);
            foreach (var ch in fileName)
            {
                sb.Append(invalid.Contains(ch) ? '_' : ch);
            }
            return sb.ToString().Replace(' ', '_');
        }

        private static string EscapeHtml(string value)
        {
            return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string BuildEmailBody(string heading, string paragraph, string actionUrl, string actionText)
        {
            var safeHeading = EscapeHtml(heading);
            var safeActionText = EscapeHtml(actionText);
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f4f4f9; color: #333; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #fff; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); }}
                    .header {{ text-align: center; padding: 10px 0; background-color: #4CAF50; color: #fff; }}
                    .content {{ padding: 20px; }}
                    .button {{ display: inline-block; margin: 20px 0; padding: 10px 16px; background-color: #4CAF50; color: #fff; text-decoration: none; border-radius: 6px; }}
                    .footer {{ text-align: center; padding: 10px 0; font-size: 12px; color: #999; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>{safeHeading}</h1>
                    </div>
                    <div class='content'>
                        <p>{paragraph}</p>
                        <a class='button' target='_blank' href='{actionUrl}'>{safeActionText}</a>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2026 Controle Certo. Todos os direitos reservados.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private static TicketResponse MapTicket(Ticket ticket)
        {
            return new TicketResponse
            {
                Id = ticket.Id,
                UserId = ticket.UserId,
                Subject = ticket.Subject,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                ClosedAt = ticket.ClosedAt,
            };
        }

        private static TicketDetailResponse MapTicketDetail(Ticket ticket)
        {
            return new TicketDetailResponse
            {
                Id = ticket.Id,
                UserId = ticket.UserId,
                Subject = ticket.Subject,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                ClosedAt = ticket.ClosedAt,
            };
        }

        private static TicketMessageResponse MapMessage(TicketMessage message)
        {
            return new TicketMessageResponse
            {
                Id = message.Id,
                TicketId = message.TicketId,
                AuthorUserId = message.AuthorUserId,
                AuthorRole = message.AuthorRole,
                Body = message.Body,
                CreatedAt = message.CreatedAt,
                Attachments = message.Attachments is null
                    ? new List<TicketAttachmentResponse>()
                    : message.Attachments.Select(MapAttachment).ToList(),
            };
        }

        private static TicketAttachmentResponse MapAttachment(TicketAttachment attachment)
        {
            return new TicketAttachmentResponse
            {
                Id = attachment.Id,
                TicketId = attachment.TicketId,
                TicketMessageId = attachment.TicketMessageId,
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                SizeBytes = attachment.SizeBytes,
                Url = attachment.Url,
                StorageKey = attachment.StorageKey,
                CreatedAt = attachment.CreatedAt,
            };
        }
    }
}

