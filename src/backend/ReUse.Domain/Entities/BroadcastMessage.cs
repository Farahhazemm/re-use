using ReUse.Domain.Enums;

namespace ReUse.Domain.Entities;

public class BroadcastMessage : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;

    public BroadcastAudience TargetAudience { get; set; }
    public BroadcastStatus Status { get; set; } = BroadcastStatus.Draft;

    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }

    public int RecipientCount { get; set; }
    public int DeliveredCount { get; set; }
    public int FailedCount { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public uint RowVersion { get; set; }
}