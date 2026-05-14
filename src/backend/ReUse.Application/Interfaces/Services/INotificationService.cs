using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Notification;

namespace ReUse.Application.Interfaces.Services;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetUserNotifications(Guid userId, PaginationParams pagination);
    Task<int> GetUnreadCount(Guid userId);
    Task MarkAsRead(Guid userId, Guid notificationId);
}