using ReUse.Application.Enums;

namespace ReUse.Application.DTOs.Users.User_Management;

public record UpdateAdminUserRequest
{
    public string? FullName { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Bio { get; init; }
    public string? AddressLine1 { get; init; }
    public string? City { get; init; }
    public string? StateProvince { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public UserRole? Role { get; init; }
}