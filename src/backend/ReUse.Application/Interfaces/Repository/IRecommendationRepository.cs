using ReUse.Application.DTOs.Recommendations;
using ReUse.Domain.Entities;
using ReUse.Domain.Enums;

namespace ReUse.Application.Interfaces.Repository;

public interface IRecommendationRepository
{
    Task<UserRecommendationContext> GetUserContextAsync(Guid? userId);

    Task<IReadOnlyList<CandidateProduct>> GetCandidatesAsync(UserRecommendationContext context);

    Task<IReadOnlyList<CandidateProduct>> GetSimilarCandidatesAsync(Guid productId, Guid categoryId, Guid? parentCategoryId, Guid? excludeUserId, int count = 20);

    Task<IReadOnlyList<Product>> GetProductsByIdsAsync(IEnumerable<Guid> orderedIds);

    Task<(Guid CategoryId, Guid? ParentCategoryId, ProductCondition? Condition, string Title)?> GetProductCategoryInfoAsync(Guid productId);
}