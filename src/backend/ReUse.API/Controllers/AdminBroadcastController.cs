using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ReUse.API.Extensions;
using ReUse.API.Responses;
using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Broadcast;
using ReUse.Application.Interfaces.Services;

namespace ReUse.API.Controllers;

[ApiController]
[Route("api/admin/broadcasts")]
[Authorize(Roles = "Admin")]
[Tags("Broadcasts - Admin")]
public class AdminBroadcastController : ControllerBase
{
    private readonly IAdminBroadcastService _broadcastService;

    public AdminBroadcastController(IAdminBroadcastService broadcastService)
    {
        _broadcastService = broadcastService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BroadcastResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] BroadcastFilterParams filterParams)
    {
        var result = await _broadcastService.GetAllAsync(filterParams);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BroadcastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _broadcastService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(BroadcastSummaryStats), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _broadcastService.GetSummaryStatsAsync();
        return Ok(result);
    }

    [HttpPost("draft")]
    [ProducesResponseType(typeof(BroadcastResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDraft([FromForm] CreateBroadcastRequest request)
    {
        var adminId = User.GetBusinessId();
        var result = await _broadcastService.CreateDraftAsync(request, adminId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}/draft")]
    [ProducesResponseType(typeof(BroadcastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDraft(Guid id, [FromForm] UpdateBroadcastRequest request)
    {
        var adminId = User.GetBusinessId();
        var result = await _broadcastService.UpdateDraftAsync(id, request, adminId);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDraft(Guid id)
    {
        var adminId = User.GetBusinessId();
        await _broadcastService.DeleteAsync(id, adminId);
        return NoContent();
    }

    [HttpPost("send")]
    [ProducesResponseType(typeof(BroadcastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendNow([FromForm] CreateBroadcastRequest request)
    {
        var adminId = User.GetBusinessId();
        var result = await _broadcastService.SendAsync(request, adminId);
        return Ok(result);
    }

    [HttpPost("schedule")]
    [ProducesResponseType(typeof(BroadcastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Schedule([FromBody] CreateBroadcastRequest request)
    {
        var adminId = User.GetBusinessId();
        var result = await _broadcastService.ScheduleAsync(request, adminId);
        return Ok(result);
    }
}