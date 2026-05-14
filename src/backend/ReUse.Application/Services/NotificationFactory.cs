using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using ReUse.Application.Interfaces.Services;
using ReUse.Domain.Entities;
using ReUse.Domain.Enums;

namespace ReUse.Application.Services;

public class NotificationFactory : INotificationFactory
{
    public Notification Create<T>(Guid userId, NotificationType type, string title, string body, T data, Guid? correlationId = null, Guid? causationId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            CreatedAt = DateTime.UtcNow,
            CorrelationId = correlationId ?? Guid.NewGuid(),
            CausationId = causationId,
            Deliveries = new List<NotificationDelivery>()
        };

        // Convert data 
        if (data != null)
        {
            notification.Data = JsonSerializer.Serialize(data);
        }

        notification.Metadata = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["source"] = "FollowService"
        });

        // In  App Delivery Record
        notification.Deliveries.Add(new NotificationDelivery
        {
            Channel = NotificationChannel.InApp,
            Status = DeliveryStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });

        return notification;
    }
}