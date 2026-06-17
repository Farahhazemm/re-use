using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.DTOs.Notification.NotificationData;

public class BroadcastNotificationData : INotificationData
{
    public Guid BroadcastId { get; set; }
}