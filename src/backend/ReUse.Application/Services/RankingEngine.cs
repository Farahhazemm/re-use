using ReUse.Application.DTOs.Recommendations;

namespace ReUse.Application.Services;

public static class RankingEngine
{
    private const double WeightCategoryAffinity = 0.40;
    private const double WeightFreshness = 0.30;
    private const double WeightSellerAffinity = 0.20;
    private const double WeightLocation = 0.10;

    public static double Score(CandidateProduct candidate, UserRecommendationContext context)
    {
        var categoryAffinity = CategoryAffinityScore(candidate, context);
        var freshness = FreshnessScore(candidate.CreatedAt);
        var sellerAffinity = SellerAffinityScore(candidate, context);
        var location = LocationScore(candidate, context);

        return WeightCategoryAffinity * categoryAffinity
             + WeightFreshness * freshness
             + WeightSellerAffinity * sellerAffinity
             + WeightLocation * location;
    }

    public static double CategoryAffinityScore(CandidateProduct candidate, UserRecommendationContext context)
    {
        if (context.FollowedCategoryIds.Contains(candidate.CategoryId))
            return 1.00;

        if (context.TopFavoritedCategoryIds.Contains(candidate.CategoryId))
            return 0.75;

        return 0.00;
    }

    // fix monotonic exponential decay no buckets no jumps
    public static double FreshnessScore(DateTime createdAt)
    {
        var daysOld = (DateTime.UtcNow - createdAt).TotalDays;

        if (daysOld < 0)
            daysOld = 0;

        const double lambda = 0.08; // tuning parameter

        return Math.Exp(-lambda * daysOld);
    }

    public static double SellerAffinityScore(CandidateProduct candidate, UserRecommendationContext context)
    {
        return context.FollowingSellerIds.Contains(candidate.OwnerUserId) ? 1.00 : 0.00;
    }

    public static double LocationScore(CandidateProduct candidate, UserRecommendationContext context)
    {
        if (string.IsNullOrEmpty(context.UserCity) && string.IsNullOrEmpty(context.UserCountry))
            return 0.00;

        var productCity = candidate.LocationCity?.Trim().ToLowerInvariant();
        var productCountry = candidate.LocationCountry?.Trim().ToLowerInvariant();
        var userCity = context.UserCity?.Trim().ToLowerInvariant();
        var userCountry = context.UserCountry?.Trim().ToLowerInvariant();

        if (!string.IsNullOrEmpty(userCity)
            && !string.IsNullOrEmpty(productCity)
            && productCity == userCity)
            return 1.00;

        if (!string.IsNullOrEmpty(userCountry)
            && !string.IsNullOrEmpty(productCountry)
            && productCountry == userCountry)
            return 0.60;

        return 0.00;
    }
}