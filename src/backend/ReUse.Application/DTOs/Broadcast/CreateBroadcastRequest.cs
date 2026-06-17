using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.Broadcast;

public class CreateBroadcastRequest
{
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public BroadcastAudience TargetAudience { get; set; }
    public DateTime? ScheduledAt { get; set; }  // null = send immediately
}