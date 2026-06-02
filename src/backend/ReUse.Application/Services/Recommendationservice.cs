using AutoMapper;

using Microsoft.Extensions.Options;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Products.Responses;
using ReUse.Application.DTOs.Recommendations;
using ReUse.Application.Exceptions;
using ReUse.Application.Interfaces;
using ReUse.Application.Interfaces.Repository;
using ReUse.Application.Interfaces.Services;
using ReUse.Application.Options;

namespace ReUse.Application.Services;

public class RecommendationService : IRecommendationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly RecommendationWeights _weights;

    public RecommendationService(IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<RecommendationWeights> weights)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _weights = weights.Value;
    }

    #region Personalised Feed
    public async Task<PagedResult<ProductResponse>> GetPersonalisedFeedAsync(
        Guid? userId,
        PaginationParams @params)
    {
        var context = await _unitOfWork.Recommendations.GetUserContextAsync(userId);
        var candidates = await _unitOfWork.Recommendations.GetCandidatesAsync(context);

        var scored = candidates
            .Select(c => new ScoredProduct
            {
                Candidate = c,
                Score = RankingEngine.Score(c, context, _weights)
            })
            .OrderByDescending(s => s.Score)
            .ToList();

        var totalRecords = scored.Count;
        var pageNumber = @params.PageNumber;
        var pageSize = @params.PageSize;

        var pageIds = scored
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => s.Candidate.Id)
            .ToList();

        var products = await _unitOfWork.Recommendations.GetProductsByIdsAsync(pageIds);
        var data = _mapper.Map<List<ProductResponse>>(products);

        return new PagedResult<ProductResponse>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        };
    }
    #endregion

    #region Similar Products
    public async Task<IReadOnlyList<ProductResponse>> GetSimilarProductsAsync(
        Guid productId,
        Guid? userId,
        int count = 8)
    {
        if (count is < 1 or > 50)
            throw new InvalidRequestException("Count must be between 1 and 50.");

        var categoryInfo = await _unitOfWork.Recommendations.GetProductCategoryInfoAsync(productId);

        if (categoryInfo is null)
            throw new NotFoundException($"Product {productId} not found or not active.");

        var (categoryId, parentCategoryId, condition, title) = categoryInfo.Value;

        var candidates = await _unitOfWork.Recommendations.GetSimilarCandidatesAsync(
            productId, categoryId, parentCategoryId, excludeUserId: userId, count: count * 3);

        if (candidates.Count == 0)
            return [];

        // Use optional user context so location scoring can apply if user is known
        var context = await _unitOfWork.Recommendations.GetUserContextAsync(userId);

        var topIds = candidates
            .Select(c => new
            {
                Id = c.Id,
                Score = RankingEngine.SimilarityScore(c, categoryId, parentCategoryId, condition, title, context)
            })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .Select(x => x.Id)
            .ToList();

        var products = await _unitOfWork.Recommendations.GetProductsByIdsAsync(topIds);
        return _mapper.Map<List<ProductResponse>>(products);
    }
    #endregion
}