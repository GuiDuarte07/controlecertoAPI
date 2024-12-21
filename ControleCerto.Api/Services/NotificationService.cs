using AutoMapper;
using ControleCerto.DTOs.Notification;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;

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

        public async Task<Result<ICollection<InfoNotificationResponse>>> GetRecentNotificationsAsync(int userId, bool? isRead)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return new AppError("Usuário não encontrado.", ErrorTypeEnum.NotFound);
            }

            var notifications = await _appDbContext.Notifications
                .Where(n => n.ExpiresAt >= DateTime.UtcNow && n.UserId == userId)
                .OrderBy(n => n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => 
                    _mapper.Map<InfoNotificationResponse>(n))
                .ToListAsync();

            if (isRead is not null)
            {
                notifications = notifications.Where(n => n.IsRead == isRead).ToList();
            }

            return notifications;
        }

        public async Task<Result<ICollection<InfoNotificationResponse>>> GetAllNotificationsAsync(int userId)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return new AppError("Usuário não encontrado.", ErrorTypeEnum.NotFound);
            }

            var notifications = await _appDbContext.Notifications
                .Where(n => n.UserId == userId)
                .OrderBy(n => n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n =>
                    _mapper.Map<InfoNotificationResponse>(n))
            .ToListAsync();

            return notifications;
        }

        public async Task<Result<InfoNotificationResponse>> SendUserNotificationAsync(CreateNotificationRequest notification, int userId)
        {
            try
            {
                //Depois fazer a validação de usuário!!!
                var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user is null || userId != notification.UserId || user.IsAdmin == false)
                {
                    return new AppError("Usuário não autorizado.", ErrorTypeEnum.Validation);
                }


                var notificationToCreate = _mapper.Map<Notification>(notification);

                if (notification.ExpiresAt is null)
                {
                    var expiresAt = DateTime.UtcNow.AddDays(7);
                    notificationToCreate.ExpiresAt = expiresAt;
                }

                var createdNotification = await _appDbContext.Notifications.AddAsync(notificationToCreate);
                await _appDbContext.SaveChangesAsync();

                return _mapper.Map<InfoNotificationResponse>(createdNotification.Entity);
            }
            catch (Exception ex)
            {
                return new AppError($"Algum problema aconteceu ao enviar essa notificação: {ex.Message}", ErrorTypeEnum.InternalError);
            }

        }

        public async Task<Result<bool>> SendPublicNotificationAsync(CreatePublicNotificationRequest notification, int? adminId)
        {
            if (adminId is not null)
            {
                var admin = await _appDbContext.Users.FirstOrDefaultAsync(user => user.Id == adminId);

                if (admin == null || admin.IsAdmin == false)
                {
                    return new AppError("Metódo não suportado para esse usuário.", ErrorTypeEnum.BusinessRule);
                }
            }

            var userIds = await _appDbContext.Users.Select(u => u.Id).ToListAsync();

            var expiresAt = DateTime.UtcNow.AddDays(7);

            var notifications = userIds.Select(userId => 
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

            return true;
        }

        public async Task<Result<bool>> MarkAsReadAsync(ICollection<long> notificationIds, int userId)
        {
            try
            {
                var notifications = await _appDbContext.Notifications
                .Where(n => notificationIds.Contains(n.Id) && n.UserId == userId)
                .ToListAsync();

                if (notifications == null || notificationIds.Count == 0)
                {
                    return new AppError("Nenhuma notificação encontrada para o usuário especificado.", ErrorTypeEnum.NotFound);
                }


                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new AppError($"Erro ao marcar notificações como lidas: {ex.Message}", ErrorTypeEnum.InternalError);
            }

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
