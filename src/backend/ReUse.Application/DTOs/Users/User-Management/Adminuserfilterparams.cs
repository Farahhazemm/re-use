using ReUse.Application.Enums;

namespace ReUse.Application.DTOs.Users.Admin;

public class AdminUserFilterParams
{
    public PaginationParams Pagination { get; set; } = new();

    public UserSortBy SortBy { get; set; } = UserSortBy.CreatedAt;
    public SortDirection SortOrder { get; set; } = SortDirection.Desc;

    public string? SearchTerm { get; set; }

    public string? City { get; set; }
    public string? Country { get; set; }
    public string? StateProvince { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    public UserRole? Role { get; set; }
}