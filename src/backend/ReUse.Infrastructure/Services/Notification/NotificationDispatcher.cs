using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using ReUse.Application.DTOs.Notification;
using ReUse.Application.Interfaces;
using ReUse.Application.Interfaces.Services;
using ReUse.Domain.Entities;
using ReUse.Infrastructure.Notifications.SignalR;

namespace ReUse.Application.Services;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(IUnitOfWork unitOfWork, IHubContext<NotificationHub> hubContext, ILogger<NotificationDispatcher> logger)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task DispatchAsync(Notification notification)
    {
        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        // Real-time Push
        var dto = MapToDto(notification);
        await _hubContext.Clients.User(notification.UserId.ToString())
                         .SendAsync("ReceiveNotification", dto);

        _logger.LogInformation("Notification sent to user {UserId} - Type: {Type}",
            notification.UserId, notification.Type);
    }

    private NotificationDto MapToDto(Notification n)
    {
        return new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            Body = n.Body,
            Type = n.Type.ToString(),
            Data = n.Data,
            Metadata = n.Metadata,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ReadAt = n.ReadAt
        };
    }
}