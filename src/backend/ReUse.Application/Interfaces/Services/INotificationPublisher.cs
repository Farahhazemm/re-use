using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Domain.Enums;

namespace ReUse.Application.Interfaces.Services;

public interface INotificationPublisher
{
    Task PublishAsync<T>(Guid userId, NotificationType type, string title, string body, T data, Guid? correlationId = null, Guid? causationId = null);

    Task PublishToMultipleAsync<T>(IEnumerable<Guid> userIds, NotificationType type, string title, string body, T data);
}