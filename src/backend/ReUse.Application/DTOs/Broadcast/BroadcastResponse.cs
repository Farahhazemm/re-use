using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.DTOs.Broadcast;

public record BroadcastResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string Body { get; init; } = null!;
    public string TargetAudience { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime? ScheduledAt { get; init; }
    public DateTime? SentAt { get; init; }
    public int RecipientCount { get; init; }
    public int DeliveredCount { get; init; }
    public int FailedCount { get; init; }
    public string CreatedBy { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}