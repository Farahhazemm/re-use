using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ReUse.API.Extensions;
using ReUse.API.Responses;
using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Products;
using ReUse.Application.DTOs.Products.Requests;
using ReUse.Application.DTOs.Products.Responses;
using ReUse.Application.Interfaces.Services;

namespace ReUse.API.Controllers;

/// <summary>Admin endpoints for managing products</summary>
[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
[Tags("Products - Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IPromotionService _promotionService;
    private readonly ISystemActivityLogService _activityLog;

    public AdminProductsController(
        IProductService productService,
        IPromotionService promotionService,
        ISystemActivityLogService activityLog)
    {
        _productService = productService;
        _promotionService = promotionService;
        _activityLog = activityLog;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] AdminProductFilterParams filterParams)
    {
        var result = await _productService.GetAllForAdminAsync(filterParams);
        return Ok(result);
    }

    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid productId)
    {
        var result = await _productService.GetForAdminByIdAsync(productId);
        return Ok(result);
    }

    /// <summary>Delete a product (Admin). Soft-delete by setting Status to Deleted.</summary>
    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid productId)
    {
        var adminId = User.GetBusinessId();
        await _productService.DeleteProductAsync(productId, Guid.Empty, isAdmin: true);
        await _activityLog.LogProductDeletedByAdminAsync(adminId, productId);
        return Ok(new { message = "Product deleted successfully" });
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(AdminProductsSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _productService.GetAdminSummaryAsync();
        return Ok(result);
    }

    [HttpPatch("regular/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRegular(Guid productId, [FromBody] UpdateRegularProductRequest request)
    {
        await _productService.UpdateRegularProductAsync(productId, request, Guid.Empty, isAdmin: true);
        return NoContent();
    }

    [HttpPatch("swap/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSwap(Guid productId, [FromBody] UpdateSwapProductRequest request)
    {
        await _productService.UpdateSwapProductAsync(productId, request, Guid.Empty, isAdmin: true);
        return NoContent();
    }

    [HttpPatch("wanted/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWanted(Guid productId, [FromBody] UpdateWantedProductRequest request)
    {
        await _productService.UpdateWantedProductAsync(productId, request, Guid.Empty, isAdmin: true);
        return NoContent();
    }

    /// <summary>Change a product status (Admin moderation).</summary>
    [HttpPatch("{productId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(Guid productId, [FromBody] ChangeProductStatusRequest request)
    {
        var adminId = User.GetBusinessId();
        await _productService.ChangeProductStatusByAdminAsync(productId, request.Status);
        await _activityLog.LogProductModerationAsync(adminId, productId, request.Status);
        return NoContent();
    }

    /// <summary>Restore a previously deleted product (Admin).</summary>
    [HttpPatch("{productId:guid}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(Guid productId)
    {
        var adminId = User.GetBusinessId();
        await _productService.RestoreProductByAdminAsync(productId);
        await _activityLog.LogProductModerationAsync(adminId, productId, Domain.Enums.ProductStatus.Active, reason: "Restored by admin.");
        return NoContent();
    }

    /// <summary>Promote a product to premium (Admin). Bypasses payment.</summary>
    [HttpPatch("{productId:guid}/premium")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPremium(Guid productId, [FromBody] PremiumRequest request)
    {
        var adminId = User.GetBusinessId();
        await _promotionService.SetPremiumAsync(productId, request.DurationDays);
        await _activityLog.LogPremiumGrantedByAdminAsync(adminId, productId, request.DurationDays);
        return NoContent();
    }

    /// <summary>Remove premium status from a product (Admin).</summary>
    [HttpDelete("{productId:guid}/premium")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePremium(Guid productId)
    {
        var adminId = User.GetBusinessId();
        await _promotionService.RemovePremiumAsync(productId);
        await _activityLog.LogPremiumRemovedByAdminAsync(adminId, productId);
        return NoContent();
    }
}