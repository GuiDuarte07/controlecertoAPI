using AutoMapper;
using ControleCerto.DTOs.Notification;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        public NotificationService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<Result<ICollection<InfoNotificationResponse>>> GetRecentNotificationsAsync(int userId, bool? isRead, int maxNotifications = 5)
        {
            var user = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return new AppError("Usuário não encontrado.", ErrorTypeEnum.NotFound);
            }

            if (maxNotifications <= 0 || maxNotifications > 200)
            {
                return new AppError("Parâmetro de quantidade máxima inválido. Use um valor entre 1 e 200.", ErrorTypeEnum.Validation);
            }

            var query = _appDbContext.Notifications
                .AsNoTracking()
                .Where(n => n.ExpiresAt >= DateTime.UtcNow && n.UserId == userId);

            if (isRead is not null)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            var notifications = await query
                .OrderBy(n => n.IsRead)
                .ThenByDescending(n => n.CreatedAt)
                .Take(maxNotifications)
                .ToListAsync();

            var response = notifications
                .Select(n => _mapper.Map<InfoNotificationResponse>(n))
                .ToList();

            return response;
        }

        public async Task<Result<ICollection<InfoNotificationResponse>>> GetAllNotificationsAsync(int userId, int page = 1, int pageSize = 50, bool includeExpired = false)
        {
            var user = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return new AppError("Usuário não encontrado.", ErrorTypeEnum.NotFound);
            }

            if (page <= 0)
            {
                return new AppError("Parâmetro de página inválido.", ErrorTypeEnum.Validation);
            }

            if (pageSize <= 0 || pageSize > 200)
            {
                return new AppError("Parâmetro de tamanho de página inválido. Use um valor entre 1 e 200.", ErrorTypeEnum.Validation);
            }

            var query = _appDbContext.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId);

            if (!includeExpired)
            {
                query = query.Where(n => n.ExpiresAt >= DateTime.UtcNow);
            }

            var notifications = await query
                .OrderBy(n => n.IsRead)
                .ThenByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = notifications
                .Select(n => _mapper.Map<InfoNotificationResponse>(n))
                .ToList();

            return response;
        }

        public async Task<Result<ICollection<InfoNotificationResponse>>> SendUserNotificationAsync(CreateNotificationRequest notification, int userId)
        {
            var admin = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

            if (admin is null || admin.IsAdmin == false)
            {
                return new AppError("Usuário não autorizado.", ErrorTypeEnum.Validation);
            }

            var requestedTargetIds = new HashSet<int>();

            if (notification.UserId.HasValue)
            {
                requestedTargetIds.Add(notification.UserId.Value);
            }

            if (notification.UserIds is not null)
            {
                foreach (var targetId in notification.UserIds)
                {
                    requestedTargetIds.Add(targetId);
                }
            }

            requestedTargetIds.RemoveWhere(id => id <= 0 || id == userId);

            if (requestedTargetIds.Count == 0)
            {
                return new AppError("Informe ao menos um usuário de destino válido.", ErrorTypeEnum.Validation);
            }

            var existingTargetIds = await _appDbContext.Users
                .AsNoTracking()
                .Where(u => requestedTargetIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();

            if (existingTargetIds.Count != requestedTargetIds.Count)
            {
                return new AppError("Um ou mais usuários de destino não foram encontrados.", ErrorTypeEnum.NotFound);
            }

            var expiresAt = notification.ExpiresAt ?? DateTime.UtcNow.AddDays(7);
            var notificationsToCreate = existingTargetIds
                .Select(targetId => new Notification(
                    notification.Title,
                    notification.Message,
                    notification.Type,
                    notification.ActionPath,
                    expiresAt,
                    targetId
                ))
                .ToList();

            await _appDbContext.Notifications.AddRangeAsync(notificationsToCreate);
            await _appDbContext.SaveChangesAsync();

            var response = notificationsToCreate
                .Select(n => _mapper.Map<InfoNotificationResponse>(n))
                .ToList();

            return response;
        }

        public async Task<Result<bool>> SendPublicNotificationAsync(CreatePublicNotificationRequest notification, int adminId)
        {
            var admin = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == adminId);

            if (admin == null || admin.IsAdmin == false)
            {
                return new AppError("Metódo não suportado para esse usuário.", ErrorTypeEnum.BusinessRule);
            }

            var userIds = await _appDbContext.Users.Select(u => u.Id).ToListAsync();
            if (userIds.Count == 0)
            {
                return true;
            }

            var expiresAt = DateTime.UtcNow.AddDays(7);
            const int batchSize = 1000;

            for (var index = 0; index < userIds.Count; index += batchSize)
            {
                var idsBatch = userIds.Skip(index).Take(batchSize);
                var notifications = idsBatch.Select(userId =>
                    new Notification(
                        notification.Title,
                        notification.Message,
                        notification.Type,
                        notification.ActionPath,
                        notification.ExpiresAt ?? expiresAt,
                        userId
                    )
                );

                await _appDbContext.Notifications.AddRangeAsync(notifications);
                await _appDbContext.SaveChangesAsync();
            }

            return true;
        }

        public async Task<Result<bool>> MarkAsReadAsync(ICollection<long> notificationIds, int userId)
        {
            if (notificationIds is null || notificationIds.Count == 0)
            {
                return new AppError("Informe ao menos uma notificação para marcação de leitura.", ErrorTypeEnum.Validation);
            }

            var ids = notificationIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return new AppError("Informe ao menos uma notificação para marcação de leitura.", ErrorTypeEnum.Validation);
            }

            var notifications = await _appDbContext.Notifications
                .Where(n => ids.Contains(n.Id) && n.UserId == userId)
                .ToListAsync();

            if (notifications.Count == 0)
            {
                return new AppError("Nenhuma notificação encontrada para o usuário especificado.", ErrorTypeEnum.NotFound);
            }

            foreach (var notification in notifications)
            {
                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                }
            }

            await _appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<Result<bool>> DeleteNotificationAsync(long notificationId, int userId)
        {
            var notification = await _appDbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return new AppError("Notificação não encontrada", ErrorTypeEnum.NotFound);
            }

            _appDbContext.Notifications.Remove(notification);

            await _appDbContext.SaveChangesAsync();

            return true;
        }
   
    }
}
