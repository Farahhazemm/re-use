using ReUse.Application.Enums;
using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.Reports;

public class AdminReportFilterParams
{
    private DateTime? _createdFrom;
    private DateTime? _createdTo;

    public PaginationParams Pagination { get; set; } = new();

    public SortDirection SortDirection { get; set; } = SortDirection.Desc;

    public ReportStatus? Status { get; set; }

    public ReportTargetType? TargetType { get; set; }

    public Guid? ReporterUserId { get; set; }

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

    private static DateTime? NormalizeToUtc(DateTime? value)
    {
        if (!value.HasValue) return null;

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc),
        };
    }
}