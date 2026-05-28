using AutoMapper;

using ReUse.Application.DTOs.Users.Admin;
using ReUse.Application.DTOs.Users.User_Management;
using ReUse.Application.Mappers;
using ReUse.Domain.Entities;

namespace ReUse.Application.Mappers;

public class AdminUserMappingProfile : Profile
{
    public AdminUserMappingProfile()
    {

        CreateMap<AdminUserMappingSource, AdminUserResponse>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.DomainUser.Id))
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.DomainUser.FullName))
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => src.DomainUser.Email))
            .ForMember(dest => dest.PhoneNumber,
                opt => opt.MapFrom(src => src.DomainUser.PhoneNumber))
            .ForMember(dest => dest.ProfileImageUrl,
                opt => opt.MapFrom(src => src.DomainUser.ProfileImageUrl))
            .ForMember(dest => dest.City,
                opt => opt.MapFrom(src => src.DomainUser.City))
            .ForMember(dest => dest.Country,
                opt => opt.MapFrom(src => src.DomainUser.Country))
            .ForMember(dest => dest.IsActive,
                opt => opt.MapFrom(src => src.DomainUser.IsActive))
            .ForMember(dest => dest.DeactivatedAt,
                opt => opt.MapFrom(src => src.DomainUser.DeactivatedAt))
            .ForMember(dest => dest.CreatedAt,
                opt => opt.MapFrom(src => src.DomainUser.CreatedAt))
            //// Identity fields
            //.ForMember(dest => dest.Username,
            //    opt => opt.MapFrom(src => src.IdentityUser != null ? src.IdentityUser.UserName : null))
            //.ForMember(dest => dest.EmailConfirmed,
            //    opt => opt.MapFrom(src => src.IdentityUser != null && src.IdentityUser.EmailConfirmed))
            //.ForMember(dest => dest.PhoneNumberConfirmed,
            //    opt => opt.MapFrom(src => src.IdentityUser != null && src.IdentityUser.PhoneNumberConfirmed))
            //.ForMember(dest => dest.LockoutEnabled,
            //    opt => opt.MapFrom(src => src.IdentityUser != null && src.IdentityUser.LockoutEnabled))
            //.ForMember(dest => dest.LockoutEnd,
            //    opt => opt.MapFrom(src => src.IdentityUser != null ? src.IdentityUser.LockoutEnd : null))
            //.ForMember(dest => dest.AccessFailedCount,
            //    opt => opt.MapFrom(src => src.IdentityUser != null ? src.IdentityUser.AccessFailedCount : 0))
            .ForMember(dest => dest.Roles,
                opt => opt.MapFrom(src => src.Roles));
    }
}


public class AdminUserMappingSource
{
    public User DomainUser { get; init; } = null!;

    // public ApplicationUser? IdentityUser { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = [];
}