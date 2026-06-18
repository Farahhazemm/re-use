using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.Broadcast;

public class UpdateBroadcastRequest
{
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public BroadcastAudience TargetAudience { get; set; }
    public DateTime? ScheduledAt { get; set; }
}