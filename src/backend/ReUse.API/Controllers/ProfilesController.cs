using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ReUse.API.Responses;
using ReUse.Application.DTOs.Users.UserProfile;
using ReUse.Application.Interfaces.Services;

namespace ReUse.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProfilesController : ControllerBase
{
    private readonly IUserService _userService;

    public ProfilesController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [AllowAnonymous]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        var userProfile = await _userService.GetUserProfileAsync(userId);
        return Ok(userProfile);
    }
}