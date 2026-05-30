using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ReUse.API.Extensions;
using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Products.Responses;
using ReUse.Application.Interfaces.Services;

namespace ReUse.API.Controllers;

[ApiController]
[Route("api/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet("feed")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeed([FromQuery] PaginationParams @params)
    {
        var userId = User.Identity?.IsAuthenticated == true
             ? User.GetBusinessId()
             : (Guid?)null;
        var result = await _recommendationService.GetPersonalisedFeedAsync(userId, @params);
        return Ok(result);
    }
}