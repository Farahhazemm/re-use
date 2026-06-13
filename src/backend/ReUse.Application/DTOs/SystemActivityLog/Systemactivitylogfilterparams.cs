using ReUse.Application.Enums;
using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.SystemActivityLog;

public class SystemActivityLogFilterParams
{
    private DateTime? _createdFrom;
    private DateTime? _createdTo;

    public PaginationParams Pagination { get; set; } = new();
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;

    public string? SortBy { get; set; }

    public Guid? ActorUserId { get; set; }

    public LogActionType? ActionType { get; set; }
    public LogCategory? Category { get; set; }
    public LogSeverity? Severity { get; set; }
    public LogStatus? Status { get; set; }

    public string? EntityType { get; set; }
    public string? EntityId { get; set; }

    public DateTime? CreatedFrom
    {
        get => _createdFrom;
        set => _createdFrom = NormalizeToUtc(value);
    }

    public DateTime? CreatedTo
    {
        get => _createdTo;
        set => _createdTo = NormalizeToUtc(value);
    }

    public string? Search { get; set; }

    public string? DescriptionSearch { get; set; }

    private static DateTime? NormalizeToUtc(DateTime? value)
    {
        if (!value.HasValue) return null;

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Local).ToUniversalTime(),

        };
    }
}