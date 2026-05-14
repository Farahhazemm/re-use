using ReUse.Domain.Enums;

namespace ReUse.Domain.Entities;

public class UserNotificationSetting : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public NotificationType NotificationType { get; set; }
    public NotificationChannel Channel { get; set; }

    public bool IsEnabled { get; set; } = true;

    public TimeOnly? QuietHoursStart { get; set; }
    public TimeOnly? QuietHoursEnd { get; set; }
}