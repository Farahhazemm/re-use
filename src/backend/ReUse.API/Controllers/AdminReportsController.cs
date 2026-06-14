using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ReUse.API.Extensions;
using ReUse.API.Responses;
using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Reports;
using ReUse.Application.Interfaces.Services;

namespace ReUse.API.Controllers;


[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin")]
[Tags("Reports - Admin")]
public class AdminReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public AdminReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminReportListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] AdminReportFilterParams filterParams)
    {
        var result = await _reportService.GetAllAsync(filterParams);
        return Ok(result);
    }


    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReportDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _reportService.GetByIdAsync(id);
        return Ok(result);
    }


    [HttpPatch("{id:guid}/review")]
    [ProducesResponseType(typeof(ReportDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Review(Guid id, [FromForm] ReviewReportRequest request)
    {
        var reviewerUserId = User.GetBusinessId();
        var result = await _reportService.ReviewAsync(id, reviewerUserId, request);
        return Ok(result);
    }
}