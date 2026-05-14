using ReUse.Domain.Enums;

namespace ReUse.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;

    public NotificationType Type { get; set; }

    public string? Data { get; set; }
    public string? Metadata { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public DateTime? SeenAt { get; set; }

    public Guid? CorrelationId { get; set; }
    public Guid? CausationId { get; set; }

    public ICollection<NotificationDelivery> Deliveries { get; set; } = new List<NotificationDelivery>();
}