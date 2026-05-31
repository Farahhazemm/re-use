using AutoMapper;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Products.Responses;
using ReUse.Application.DTOs.Recommendations;
using ReUse.Application.Exceptions;
using ReUse.Application.Interfaces.Repository;
using ReUse.Application.Interfaces.Services;

namespace ReUse.Application.Services;

public class RecommendationService : IRecommendationService
{
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IMapper _mapper;

    public RecommendationService(
        IRecommendationRepository recommendationRepository,
        IMapper mapper)
    {
        _recommendationRepository = recommendationRepository;
        _mapper = mapper;
    }



    #region Personalised Feed 
    public async Task<PagedResult<ProductResponse>> GetPersonalisedFeedAsync(
   Guid? userId,
   PaginationParams @params)
    {

        var context = await _recommendationRepository.GetUserContextAsync(userId);


        var candidates = await _recommendationRepository.GetCandidatesAsync(context);


        var scored = candidates
            .Select(c => new ScoredProduct { Candidate = c, Score = RankingEngine.Score(c, context) })
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


        var products = await _recommendationRepository.GetProductsByIdsAsync(pageIds);


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
    public async Task<IReadOnlyList<ProductResponse>> GetSimilarProductsAsync(Guid productId, Guid? userId, int count = 8)
    {
        if (count < 1 || count > 50)
            throw new InvalidRequestException("Count must be between 1 and 50.");

        var categoryInfo = await _recommendationRepository.GetProductCategoryInfoAsync(productId);

        if (categoryInfo is null)
            throw new NotFoundException($"Product {productId} not found or not active.");

        var (categoryId, parentCategoryId) = categoryInfo.Value;


        var candidates = await _recommendationRepository.GetSimilarCandidatesAsync(productId, categoryId, parentCategoryId, excludeUserId: userId, count: count * 3);

        if (candidates.Count == 0)
            return [];


        var context = await _recommendationRepository.GetUserContextAsync(userId);

        var topIds = await _recommendationRepository.RankCandidatesAsync(candidates, context, count);

        var products = await _recommendationRepository.GetProductsByIdsAsync(topIds);

        return _mapper.Map<List<ProductResponse>>(products);
    }
}
#endregion