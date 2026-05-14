using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Domain.Entities;
using ReUse.Domain.Enums;

namespace ReUse.Application.Interfaces.Services;

public interface INotificationFactory
{
    Notification Create<T>(Guid userId, NotificationType type, string title, string body, T data, Guid? correlationId = null, Guid? causationId = null);
}