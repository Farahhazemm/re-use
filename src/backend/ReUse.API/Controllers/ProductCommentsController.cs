using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ReUse.API.Extensions;
using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Comments;
using ReUse.Application.Enums;
using ReUse.Application.Interfaces.Services;

namespace ReUse.API.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/comments")]
public class ProductCommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public ProductCommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<CommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComments([FromRoute] Guid productId, [FromQuery] PaginationParams pagination, [FromQuery] SortDirection sortDirection = SortDirection.Desc)
    {
        var result = await _commentService.GetProductCommentsAsync(productId, pagination, sortDirection);
        return Ok(result);
    }


    [HttpGet("{commentId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentById([FromRoute] Guid productId, [FromRoute] Guid commentId)
    {
        var result = await _commentService.GetCommentByIdAsync(productId, commentId);
        return Ok(result);
    }


    [HttpGet("{commentId}/replies")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<CommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReplies(
        [FromRoute] Guid productId,
        [FromRoute] Guid commentId,
        [FromQuery] PaginationParams pagination,
        [FromQuery] SortDirection sortDirection = SortDirection.Asc)
    {
        var result = await _commentService.GetRepliesAsync(productId, commentId, pagination, sortDirection);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment([FromRoute] Guid productId, [FromBody] CreateCommentRequest request)
    {
        var userId = User.GetBusinessId();
        var result = await _commentService.AddCommentAsync(productId, userId, request);
        return CreatedAtAction(nameof(GetCommentById), new { productId, commentId = result.Id }, result);
    }
}