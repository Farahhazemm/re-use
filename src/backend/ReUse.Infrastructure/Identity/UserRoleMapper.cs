using ReUse.Application.Enums;

namespace ReUse.Infrastructure.Identity;

public static class UserRoleMapper
{
    private static readonly Dictionary<UserRole, string> _enumToRole = new()
    {
        [UserRole.User] = "User",
        [UserRole.Admin] = "Admin",
    };
    public static string ToRoleName(UserRole role) => _enumToRole[role];

    public static string? ToRoleName(UserRole? role) =>
        role.HasValue ? _enumToRole[role.Value] : null;
}