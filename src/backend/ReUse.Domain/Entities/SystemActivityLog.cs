using ReUse.Domain.Enums;

namespace ReUse.Domain.Entities;

public class SystemActivityLog : BaseEntity
{
    // Null for system actions
    public Guid? ActorUserId { get; set; }

    // may be null when the actor account has been deleted
    public User? ActorUser { get; set; }
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


    // TODO Add ICurrentUserService (or IHttpContextAccessor wrapper) to resolve
    //       ActorUserId, IpAddress, and UserAgent automatically at the service layer
    //       rather than requiring callers to pass them explicitly.
    //
    // TODO Add IRequestContext service that surfaces: UserId, IpAddress, UserAgent,
    //       CorrelationId. Once available, inject it into SystemActivityLogService.
    //
    // TODO Consider a CorrelationId column (string?) to group related log entries
    //       that originated from the same HTTP request or background job run.
}