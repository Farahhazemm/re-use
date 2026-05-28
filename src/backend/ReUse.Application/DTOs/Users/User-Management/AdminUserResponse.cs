using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.DTOs.Users.User_Management;

// Represents a user entry in the admin user list, including identity metadata and assigned roles
public record AdminUserResponse
{
    // domain  fields 
    public Guid Id { get; init; }
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? ProfileImageUrl { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public bool IsActive { get; init; }
    public DateTime? DeactivatedAt { get; init; }
    public DateTime CreatedAt { get; init; }


    //  Role data =>populated after join with identity roles tables
    public IReadOnlyList<string> Roles { get; init; } = [];
}