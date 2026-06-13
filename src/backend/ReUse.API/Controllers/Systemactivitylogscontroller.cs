using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ReUse.API.Responses;
using ReUse.Application.DTOs;
using ReUse.Application.DTOs.SystemActivityLog;
using ReUse.Application.Interfaces.Services;

namespace ReUse.API.Controllers;

[ApiController]
[Route("api/system-activity-logs")]
[Authorize(Roles = "Admin")]
[Tags("System Activity Logs - Admin")]
public class Systemactivitylogscontroller : ControllerBase
{
    private readonly ISystemActivityLogService _logService;

    public Systemactivitylogscontroller(ISystemActivityLogService logService)
    {
        _logService = logService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SystemActivityLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] SystemActivityLogFilterParams filterParams)
    {
        var result = await _logService.GetAllAsync(filterParams);
        return Ok(result);
    }


    [HttpGet("{logId:guid}")]
    [ProducesResponseType(typeof(SystemActivityLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid logId)
    {
        var result = await _logService.GetByIdAsync(logId);
        return Ok(result);
    }
}