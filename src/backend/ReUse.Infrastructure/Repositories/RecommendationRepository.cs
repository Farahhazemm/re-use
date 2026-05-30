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


    public async Task<IReadOnlyList<CandidateProduct>> GetCandidatesAsync(UserRecommendationContext context)
    {
        var cutoffFresh = DateTime.UtcNow.AddDays(-FreshDaysThreshold);
        var cutoffTrending = DateTime.UtcNow.AddDays(-TrendingDaysThreshold);


        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active)
            .Select(p => new
            {
                p.Id,
                p.CategoryId,
                ParentCategoryId = p.Category.ParentId,
                p.OwnerUserId,
                p.Title,
                p.Condition,
                p.LocationCity,
                p.LocationCountry,
                p.CreatedAt,

                FavoriteCount = p.Favorites.Count(),
                RecentFavorites = p.Favorites.Count(f => f.CreatedAt >= cutoffTrending),
                CommentCount = p.Comments.Count(c => !c.IsDeleted)
            })
            .ToListAsync();


        var result = new List<CandidateProduct>();
        var seen = new HashSet<Guid>();

        void Add(IEnumerable<CandidateProduct> items)
        {
            foreach (var item in items)
            {
                if (seen.Add(item.Id))
                    result.Add(item);
            }
        }


        var affinityCategories = context.FollowedCategoryIds
            .Union(context.TopFavoritedCategoryIds)
            .ToHashSet();

        Add(products
            .Where(p => affinityCategories.Contains(p.CategoryId))
            .OrderByDescending(p => p.CreatedAt)
            .Take(AffinityBucketLimit)
            .Select(p => Map(p, CandidateBucket.Affinity)));


        var sellerSet = context.FollowingSellerIds.ToHashSet();

        Add(products
            .Where(p => sellerSet.Contains(p.OwnerUserId))
            .OrderByDescending(p => p.CreatedAt)
            .Take(SellerAffinityLimit)
            .Select(p => Map(p, CandidateBucket.SellerAffinity)));


        Add(products
            .OrderByDescending(p => p.RecentFavorites)
            .ThenByDescending(p => p.CreatedAt)
            .Take(TrendingBucketLimit)
            .Select(p => Map(p, CandidateBucket.Trending)));


        if (!string.IsNullOrEmpty(context.UserCity) || !string.IsNullOrEmpty(context.UserCountry))
        {
            var city = context.UserCity?.ToLower();
            var country = context.UserCountry?.ToLower();

            Add(products
                .Where(p =>
                    (city != null && p.LocationCity != null && p.LocationCity.ToLower() == city) ||
                    (country != null && p.LocationCountry != null && p.LocationCountry.ToLower() == country))
                .OrderByDescending(p => p.CreatedAt)
                .Take(LocalBucketLimit)
                .Select(p => Map(p, CandidateBucket.Local)));
        }


        Add(products
            .Where(p => p.CreatedAt >= cutoffFresh)
            .OrderByDescending(p => p.CreatedAt)
            .Take(FreshBucketLimit)
            .Select(p => Map(p, CandidateBucket.Fresh)));


        Add(products
            .OrderByDescending(p => p.FavoriteCount)
            .Take(PopularAllTimeLimit)
            .Select(p => Map(p, CandidateBucket.PopularAllTime)));

        return result;
    }


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
                FavoriteCount = p.Favorites.Count(),
                CommentCount = p.Comments.Count(c => !c.IsDeleted),
                Bucket = CandidateBucket.Fresh
            })
            .ToListAsync();
    }


    private CandidateProduct Map(dynamic p, CandidateBucket bucket)
    {
        return new CandidateProduct
        {
            Id = p.Id,
            CategoryId = p.CategoryId,
            ParentCategoryId = p.ParentCategoryId,
            OwnerUserId = p.OwnerUserId,
            Title = p.Title,
            Condition = p.Condition,
            LocationCity = p.LocationCity,
            LocationCountry = p.LocationCountry,
            CreatedAt = p.CreatedAt,
            FavoriteCount = p.FavoriteCount,
            CommentCount = p.CommentCount,
            Bucket = bucket
        };
    }


    public async Task<IReadOnlyList<Product>> GetProductsByIdsAsync(IEnumerable<Guid> orderedIds)
    {
        var ids = orderedIds.ToList();

        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
            .Include(p => p.Owner)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        return products
            .OrderBy(p => ids.IndexOf(p.Id))
            .ToList();
    }

    public async Task<(Guid CategoryId, Guid? ParentCategoryId)?> GetProductCategoryInfoAsync(Guid productId)
    {
        var row = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == productId && p.Status == ProductStatus.Active)
            .Select(p => new
            {
                p.CategoryId,
                ParentCategoryId = (Guid?)p.Category.ParentId
            })
            .FirstOrDefaultAsync();

        if (row is null)
            return null;

        return (row.CategoryId, row.ParentCategoryId);
    }


}