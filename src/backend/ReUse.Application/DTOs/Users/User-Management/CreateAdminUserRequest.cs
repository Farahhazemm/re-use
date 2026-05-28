using ReUse.Application.Enums;

namespace ReUse.Application.DTOs.Users.User_Management;

public record CreateAdminUserRequest(
    string UserName,
    string FullName,
    string Email,
    string Password,
    UserRole Role
);