using Microsoft.EntityFrameworkCore;

using ReUse.Application.DTOs.Recommendations;
using ReUse.Application.Enums;
using ReUse.Application.Interfaces.Repository;
using ReUse.Domain.Entities;
using ReUse.Domain.Enums;
using ReUse.Infrastructure.Persistence;

namespace ReUse.Infrastructure.Repositories;

public class RecommendationRepository : IRecommendationRepository
{
    private readonly ApplicationDbContext _context;

    private const int AffinityBucketLimit = 150;
    private const int SellerAffinityLimit = 50;
    private const int LocalBucketLimit = 60;
    private const int FreshBucketLimit = 80;
    private const int TrendingBucketLimit = 60;
    private const int PopularAllTimeLimit = 100;
    private const int FreshDaysThreshold = 7;
    private const int TrendingDaysThreshold = 14;

    public RecommendationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    #region UserContext
    public async Task<UserRecommendationContext> GetUserContextAsync(Guid? userId)
    {
        if (userId is null)
            return new UserRecommendationContext();

        var followedCategoryIds = await _context.CategoryFollows
            .AsNoTracking()
            .Where(cf => cf.UserId == userId)
            .Select(cf => cf.CategoryId)
            .ToListAsync();

        var topFavoritedCategoryIds = await _context.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .GroupBy(f => f.Product.CategoryId)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToListAsync();

        var followingSellerIds = await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.City, u.Country })
            .FirstOrDefaultAsync();

        return new UserRecommendationContext
        {
            UserId = userId,
            FollowedCategoryIds = followedCategoryIds,
            TopFavoritedCategoryIds = topFavoritedCategoryIds,
            FollowingSellerIds = followingSellerIds,
            UserCity = user?.City,
            UserCountry = user?.Country
        };
    }
    #endregion

    #region GetCandidates
    public async Task<IReadOnlyList<CandidateProduct>> GetCandidatesAsync(UserRecommendationContext context)
    {
        var cutoffFresh = DateTime.UtcNow.AddDays(-FreshDaysThreshold);
        var cutoffTrending = DateTime.UtcNow.AddDays(-TrendingDaysThreshold);


        var results = new List<List<CandidateProduct>>();

        if (!context.IsColdStart)
        {

            var affinityCategories = context.FollowedCategoryIds
                .Union(context.TopFavoritedCategoryIds)
                .ToList();

            results.Add(await _context.Products
                .AsNoTracking()
                .Where(p => p.Status == ProductStatus.Active
                         && affinityCategories.Contains(p.CategoryId)
                         && (context.UserId == null || p.OwnerUserId != context.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .Take(AffinityBucketLimit)
                .Select(p => new CandidateProduct
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    ParentCategoryId = p.Category.ParentId,
                    OwnerUserId = p.OwnerUserId,
                    Title = p.Title,
                    Condition = p.Condition,
                    LocationCity = p.LocationCity,
                    LocationCountry = p.LocationCountry,
                    CreatedAt = p.CreatedAt,
                    RecentFavoriteCount = p.RecentFavoriteCount,
                    CommentCount = p.Comments.Count(c => !c.IsDeleted),
                    ViewCount = p.ViewCount,
                    IsPremium = p.IsPremium,
                    PremiumExpiresAt = p.PremiumExpiresAt,
                    Bucket = CandidateBucket.Affinity
                })
                .ToListAsync());

            // Seller affinity bucket
            var sellerSet = context.FollowingSellerIds.ToList();
            if (sellerSet.Count > 0)
            {
                results.Add(await _context.Products
                    .AsNoTracking()
                    .Where(p => p.Status == ProductStatus.Active
                             && sellerSet.Contains(p.OwnerUserId)
                             && (context.UserId == null || p.OwnerUserId != context.UserId))
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(SellerAffinityLimit)
                    .Select(p => new CandidateProduct
                    {
                        Id = p.Id,
                        CategoryId = p.CategoryId,
                        ParentCategoryId = p.Category.ParentId,
                        OwnerUserId = p.OwnerUserId,
                        Title = p.Title,
                        Condition = p.Condition,
                        LocationCity = p.LocationCity,
                        LocationCountry = p.LocationCountry,
                        CreatedAt = p.CreatedAt,
                        RecentFavoriteCount = p.RecentFavoriteCount,
                        CommentCount = p.Comments.Count(c => !c.IsDeleted),
                        ViewCount = p.ViewCount,
                        IsPremium = p.IsPremium,
                        PremiumExpiresAt = p.PremiumExpiresAt,
                        Bucket = CandidateBucket.SellerAffinity
                    })
                    .ToListAsync());
            }
        }

        // Trending bucket 
        // Trending bucket
        var trendingLimit = context.IsColdStart ? TrendingBucketLimit * 2 : TrendingBucketLimit;
        results.Add(await _context.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active
                     && p.CreatedAt >= cutoffTrending
                     && (context.UserId == null || p.OwnerUserId != context.UserId))
            .OrderByDescending(p => p.RecentFavoriteCount)
            .ThenByDescending(p => p.CreatedAt)
            .Take(trendingLimit)
            .Select(p => new CandidateProduct
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                ParentCategoryId = p.Category.ParentId,
                OwnerUserId = p.OwnerUserId,
                Title = p.Title,
                Condition = p.Condition,
                LocationCity = p.LocationCity,
                LocationCountry = p.LocationCountry,
                CreatedAt = p.CreatedAt,
                RecentFavoriteCount = p.RecentFavoriteCount,
                CommentCount = p.Comments.Count(c => !c.IsDeleted),
                ViewCount = p.ViewCount,
                IsPremium = p.IsPremium,
                PremiumExpiresAt = p.PremiumExpiresAt,
                Bucket = CandidateBucket.Trending
            })
            .ToListAsync());

        // Local bucket
        if (!string.IsNullOrEmpty(context.UserCity) || !string.IsNullOrEmpty(context.UserCountry))
        {
            var city = context.UserCity?.ToLower();
            var country = context.UserCountry?.ToLower();

            results.Add(await _context.Products
                .AsNoTracking()
                .Where(p => p.Status == ProductStatus.Active
                         && (context.UserId == null || p.OwnerUserId != context.UserId)
                         && ((city != null && p.LocationCity != null && p.LocationCity.ToLower() == city)
                          || (country != null && p.LocationCountry != null && p.LocationCountry.ToLower() == country)))
                .OrderByDescending(p => p.CreatedAt)
                .Take(LocalBucketLimit)
                .Select(p => new CandidateProduct
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    ParentCategoryId = p.Category.ParentId,
                    OwnerUserId = p.OwnerUserId,
                    Title = p.Title,
                    Condition = p.Condition,
                    LocationCity = p.LocationCity,
                    LocationCountry = p.LocationCountry,
                    CreatedAt = p.CreatedAt,
                    RecentFavoriteCount = p.RecentFavoriteCount,
                    CommentCount = p.Comments.Count(c => !c.IsDeleted),
                    ViewCount = p.ViewCount,
                    IsPremium = p.IsPremium,
                    PremiumExpiresAt = p.PremiumExpiresAt,
                    Bucket = CandidateBucket.Local
                })
                .ToListAsync());
        }

        // Fresh bucket 
        var freshLimit = context.IsColdStart ? FreshBucketLimit * 2 : FreshBucketLimit;
        results.Add(await _context.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active
                     && p.CreatedAt >= cutoffFresh
                     && (context.UserId == null || p.OwnerUserId != context.UserId))
            .OrderByDescending(p => p.CreatedAt)
            .Take(freshLimit)
            .Select(p => new CandidateProduct
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                ParentCategoryId = p.Category.ParentId,
                OwnerUserId = p.OwnerUserId,
                Title = p.Title,
                Condition = p.Condition,
                LocationCity = p.LocationCity,
                LocationCountry = p.LocationCountry,
                CreatedAt = p.CreatedAt,
                RecentFavoriteCount = p.RecentFavoriteCount,
                CommentCount = p.Comments.Count(c => !c.IsDeleted),
                ViewCount = p.ViewCount,
                IsPremium = p.IsPremium,
                PremiumExpiresAt = p.PremiumExpiresAt,
                Bucket = CandidateBucket.Fresh
            })
            .ToListAsync());

        // Popularalltime bucket
        var popularLimit = context.IsColdStart ? PopularAllTimeLimit : PopularAllTimeLimit / 2;
        results.Add(await _context.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active
                     && (context.UserId == null || p.OwnerUserId != context.UserId))
            .OrderByDescending(p => p.RecentFavoriteCount)
            .Take(popularLimit)
            .Select(p => new CandidateProduct
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                ParentCategoryId = p.Category.ParentId,
                OwnerUserId = p.OwnerUserId,
                Title = p.Title,
                Condition = p.Condition,
                LocationCity = p.LocationCity,
                LocationCountry = p.LocationCountry,
                CreatedAt = p.CreatedAt,
                RecentFavoriteCount = p.RecentFavoriteCount,
                CommentCount = p.Comments.Count(c => !c.IsDeleted),
                ViewCount = p.ViewCount,
                IsPremium = p.IsPremium,
                PremiumExpiresAt = p.PremiumExpiresAt,
                Bucket = CandidateBucket.PopularAllTime
            })
            .ToListAsync());

        //  firstseen order 
        var seen = new HashSet<Guid>();
        var merged = new List<CandidateProduct>();

        foreach (var bucket in results)
        {
            foreach (var item in bucket)
            {
                if (seen.Add(item.Id))
                    merged.Add(item);
            }
        }

        return merged;
    }
    #endregion


    #region GetSimilarCandidates
    public async Task<IReadOnlyList<CandidateProduct>> GetSimilarCandidatesAsync(
        Guid productId,
        Guid categoryId,
        Guid? parentCategoryId,
        Guid? excludeUserId,
        int count = 20)
    {
        var query = _context.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active)
            .Where(p => p.Id != productId)
            .Where(p =>
                p.CategoryId == categoryId ||
                (parentCategoryId != null && p.Category.ParentId == parentCategoryId));

        if (excludeUserId.HasValue)
            query = query.Where(p => p.OwnerUserId != excludeUserId.Value);

        return await query
            .OrderByDescending(p => p.CategoryId == categoryId)
            .ThenByDescending(p => p.CreatedAt)
            .Take(count)
            .Select(p => new CandidateProduct
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                ParentCategoryId = p.Category.ParentId,
                OwnerUserId = p.OwnerUserId,
                Title = p.Title,
                Condition = p.Condition,
                LocationCity = p.LocationCity,
                LocationCountry = p.LocationCountry,
                CreatedAt = p.CreatedAt,
                RecentFavoriteCount = p.RecentFavoriteCount,
                CommentCount = p.Comments.Count(c => !c.IsDeleted),
                ViewCount = p.ViewCount,
                IsPremium = p.IsPremium,
                PremiumExpiresAt = p.PremiumExpiresAt,
                Bucket = CandidateBucket.Fresh
            })
            .ToListAsync();
    }
    #endregion

    #region GetProductsByIds
    public async Task<IReadOnlyList<Product>> GetProductsByIdsAsync(IEnumerable<Guid> orderedIds)
    {
        var ids = orderedIds.ToList();

        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
            .Include(p => p.Owner)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        var map = products.ToDictionary(p => p.Id);

        return ids
            .Where(id => map.ContainsKey(id))
            .Select(id => map[id])
            .ToList();
    }
    #endregion

    #region GetProductCategoryInfo
    public async Task<(Guid CategoryId, Guid? ParentCategoryId, ProductCondition? Condition, string Title)?> GetProductCategoryInfoAsync(Guid productId)
    {
        var row = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == productId && p.Status == ProductStatus.Active)
            .Select(p => new
            {
                p.CategoryId,
                ParentCategoryId = (Guid?)p.Category.ParentId,
                p.Condition,
                p.Title
            })
            .FirstOrDefaultAsync();

        if (row is null)
            return null;

        return (row.CategoryId, row.ParentCategoryId, row.Condition, row.Title);
    }
    #endregion
}