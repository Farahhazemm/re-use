using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Notification;
using ReUse.Domain.Entities;

namespace ReUse.Application.Interfaces.Repository;

public interface INotificationRepository : IBaseRepository<Notification>
{
    public Task<PagedResult<Notification>> GetUserNotificationsAsync(Guid userId, int page, int pageSize, bool unreadOnly = false);

    Task<int> GetUnreadCountAsync(Guid userId);

    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);

    Task MarkAllAsReadAsync(Guid userId);

    Task AddAsync(Notification notification);
}