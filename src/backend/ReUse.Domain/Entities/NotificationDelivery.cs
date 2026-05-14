using ReUse.Domain.Enums;

namespace ReUse.Domain.Entities;

public class NotificationDelivery : BaseEntity
{
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;

    public NotificationChannel Channel { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

    public DateTime? SentAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime? NextRetryAt { get; set; }

    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
}