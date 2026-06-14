using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ReUse.API.Extensions;
using ReUse.API.Responses;
using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Reports;
using ReUse.Application.Enums;
using ReUse.Application.Interfaces.Services;

namespace ReUse.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
[Tags("Reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReportDetailsResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromForm] CreateReportRequest request)
    {
        var userId = User.GetBusinessId();
        var result = await _reportService.CreateAsync(userId, request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(PagedResult<ReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyReports([FromQuery] PaginationParams pagination, [FromQuery] SortDirection sortDirection = SortDirection.Desc)
    {
        var userId = User.GetBusinessId();
        var result = await _reportService.GetByReporterAsync(userId, pagination, sortDirection);
        return Ok(result);
    }
}