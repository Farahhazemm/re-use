using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.SystemActivityLog;

// TODO: Replace explicit IpAddress / UserAgent params with IRequestContext 

public class CreateSystemActivityLogRequest
{
    public Guid? ActorUserId { get; set; }
    public string? ActorName { get; set; }

    public string? ActorEmail { get; set; }
    public LogActionType ActionType { get; set; }
    public LogCategory Category { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public LogSeverity Severity { get; set; } = LogSeverity.Info;
    public LogStatus Status { get; set; } = LogStatus.Success;
    public string Description { get; set; } = string.Empty;

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Metadata { get; set; }
}