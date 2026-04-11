using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using Microsoft.AspNetCore.Http;

using ReUse.Application.DTOs.Users.UserProfile.Commands;
using ReUse.Application.DTOs.Users.UserProfile.Contracts;
//using ReUse.Application.Utilities.Enums;

namespace ReUse.Application.Interfaces.Services.UserProfile;

public interface IUserService
{
    public Task<UserProfileDto> GetUserProfileAsync(Guid userId);

    public Task UpdateUserProfileAsync(Guid userId, UpdateUserProfileCommand command);
    // public Task UpdateImageProfileAsync(Guid userId, UpdateProfileImageCommand command);
    // public Task DeleteProfileImageAsync(Guid userId, ProfileImageType imageType);
}