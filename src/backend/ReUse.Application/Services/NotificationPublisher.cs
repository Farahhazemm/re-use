using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Application.Interfaces.Services;
using ReUse.Domain.Enums;

namespace ReUse.Application.Services;

public class NotificationPublisher : INotificationPublisher
{
    private readonly INotificationFactory _factory;
    private readonly INotificationDispatcher _dispatcher;

    public NotificationPublisher(INotificationFactory factory, INotificationDispatcher dispatcher)
    {
        _factory = factory;
        _dispatcher = dispatcher;
    }

    public async Task PublishAsync<T>(Guid userId, NotificationType type, string title, string body, T data, Guid? correlationId = null, Guid? causationId = null)
    {
        var notification = _factory.Create(userId, type, title, body, data, correlationId, causationId);
        await _dispatcher.DispatchAsync(notification);
    }

    public async Task PublishToMultipleAsync<T>(IEnumerable<Guid> userIds, NotificationType type, string title, string body, T data)
    {
        foreach (var userId in userIds)
        {
            await PublishAsync(userId, type, title, body, data);
        }
    }
}