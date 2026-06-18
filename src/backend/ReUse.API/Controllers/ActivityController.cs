using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using ReUse.API.Extensions;

using ReUse.Application.DTOs.Activity;

using ReUse.Application.Interfaces.Services;

namespace ReUse.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserActivities(Guid userId, int limit = 50)
    {
        var items = await _activityService.GetUserActivitiesAsync(userId, limit);
        return Ok(items);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyActivities(int limit = 50)
    {
        var userId = User.GetBusinessId();
        var items = await _activityService.GetUserActivitiesAsync(userId, limit);
        return Ok(items);
    }

    [HttpGet("me/history")]
    public async Task<IActionResult> GetMyActivityHistory([FromQuery] ActivityHistoryRequest request)
    {
        var userId = User.GetBusinessId();
        var result = await _activityService.GetUserActivityHistoryAsync(userId, request);
        return Ok(result);
    }

    [HttpPost("track")]
    public async Task<IActionResult> TrackActivity([FromBody] TrackActivityRequest request)
    {
        var userId = User.GetBusinessId();
        await _activityService.CreateActivityAsync(userId, request.ProductId, request.Type, request.Description, request.Metadata);
        return Ok();
    }
}