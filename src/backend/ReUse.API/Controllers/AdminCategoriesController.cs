using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ReUse.API.Extensions;
using ReUse.API.Responses;
using ReUse.Application.DTOs.Categories;
using ReUse.Application.Interfaces.Services;

namespace ReUse.API.Controllers;

/// <summary>
/// Admin endpoints for managing categories
/// </summary>
[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")]
[Tags("Categories - Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public AdminCategoriesController(ICategoryService service)
    {
        _service = service;
    }

    /// <summary>Get the full category tree including inactive categories (Admin view)</summary>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree()
    {
        var categories = await _service.GetCategoryTreeAsync(includeInactive: true);
        return Ok(categories);
    }

    /// <summary>Create a new category (root or subcategory)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest dto)
    {
        var adminId = User.GetBusinessId();
        var category = await _service.CreateAsync(dto, actorAdminId: adminId);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>Get a category by ID (Admin view)</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _service.GetByIdAsync(id);
        return Ok(category);
    }

    /// <summary>Update an existing category</summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest dto)
    {
        var adminId = User.GetBusinessId();
        var category = await _service.UpdateAsync(id, dto, actorAdminId: adminId);
        return Ok(category);
    }

    /// <summary>Delete a category</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var adminId = User.GetBusinessId();
        await _service.DeleteAsync(id, actorAdminId: adminId);
        return Ok(new { message = "Category deleted successfully" });
    }

    [HttpPatch("{id}/icon")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadIcon(Guid id, [FromForm] UploadIconRequest request)
    {
        var category = await _service.UploadIconAsync(id, request.File);
        return Ok(category);
    }
}