using ReUse.Application.DTOs.Recommendations;
using ReUse.Application.Options;
using ReUse.Domain.Enums;

namespace ReUse.Application.Services;

public static class RankingEngine
{

    // Main entry point of personalised feed scoring


    public static double Score(
        CandidateProduct candidate,
        UserRecommendationContext context,
        RecommendationWeights weights)
    {
        var multiplier = PremiumMultiplier(candidate, weights.PremiumMultiplierMax);

        var weighted =
            weights.CategoryAffinity * CategoryAffinityScore(candidate, context)
          + weights.Freshness * FreshnessScore(candidate.CreatedAt)
          + weights.Popularity * PopularityScore(candidate)
          + weights.SellerAffinity * SellerAffinityScore(candidate, context)
          + weights.Location * LocationScore(candidate, context);

        return multiplier * weighted;
    }

    // Subscores

    public static double CategoryAffinityScore(
        CandidateProduct candidate,
        UserRecommendationContext context)
    {
        if (context.FollowedCategoryIds.Contains(candidate.CategoryId))
            return 1.00;

        if (context.TopFavoritedCategoryIds.Contains(candidate.CategoryId))
            return 0.75;

        if (candidate.ParentCategoryId.HasValue)
        {
            if (context.FollowedCategoryIds.Contains(candidate.ParentCategoryId.Value))
                return 0.50;

            if (context.TopFavoritedCategoryIds.Contains(candidate.ParentCategoryId.Value))
                return 0.25;
        }

        return 0.00;
    }

    public static double FreshnessScore(DateTime createdAt)
    {
        var daysOld = Math.Max(0, (DateTime.UtcNow - createdAt).TotalDays);

        if (daysOld <= 1) return 1.00;
        if (daysOld <= 3) return 0.80;
        if (daysOld <= 7) return 0.60;

        return Math.Max(0, 0.60 - ((daysOld - 7) / 60.0));
    }


    public static double PopularityScore(CandidateProduct candidate)
    {
        var raw = (candidate.RecentFavoriteCount * 3.0)
                + (candidate.CommentCount * 2.0)
                + (candidate.ViewCount * 0.5);


        return Math.Min(Math.Log10(raw + 1) / Math.Log10(301), 1.0);
    }


    public static double SellerAffinityScore(
        CandidateProduct candidate,
        UserRecommendationContext context)
    {
        return context.FollowingSellerIds.Contains(candidate.OwnerUserId) ? 1.00 : 0.00;
    }

    public static double LocationScore(
        CandidateProduct candidate,
        UserRecommendationContext context)
    {
        if (string.IsNullOrEmpty(context.UserCity) && string.IsNullOrEmpty(context.UserCountry))
            return 0.00;

        var productCity = candidate.LocationCity?.Trim().ToLowerInvariant();
        var productCountry = candidate.LocationCountry?.Trim().ToLowerInvariant();
        var userCity = context.UserCity?.Trim().ToLowerInvariant();
        var userCountry = context.UserCountry?.Trim().ToLowerInvariant();

        if (!string.IsNullOrEmpty(userCity) && userCity == productCity)
            return 1.00;

        if (!string.IsNullOrEmpty(userCountry) && userCountry == productCountry)
            return 0.60;

        return 0.00;
    }

    public static double PremiumMultiplier(CandidateProduct candidate, double maxMultiplier)
    {
        if (candidate.IsPremium
            && candidate.PremiumExpiresAt.HasValue
            && candidate.PremiumExpiresAt.Value > DateTime.UtcNow)
        {
            return maxMultiplier;
        }
        return 1.00;
    }


    // Similar Products Scoring


    public static double SimilarityScore(
        CandidateProduct candidate,
        Guid referenceCategoryId,
        Guid? referenceParentCategoryId,
        ProductCondition? referenceCondition,
        string referenceTitle,
        UserRecommendationContext? context = null)
    {
        var category = CategorySimilarityScore(candidate, referenceCategoryId, referenceParentCategoryId);
        var condition = ConditionSimilarityScore(candidate.Condition, referenceCondition);
        var location = context is not null ? LocationScore(candidate, context) : 0.0;
        var freshness = FreshnessScore(candidate.CreatedAt);
        var keyword = TitleKeywordScore(candidate.Title, referenceTitle);

        return 0.40 * category
             + 0.25 * condition
             + 0.20 * location
             + 0.10 * freshness
             + 0.05 * keyword;
    }

    private static double CategorySimilarityScore(
        CandidateProduct candidate,
        Guid referenceCategoryId,
        Guid? referenceParentCategoryId)
    {
        if (candidate.CategoryId == referenceCategoryId)
            return 1.00;

        if (referenceParentCategoryId.HasValue
            && candidate.ParentCategoryId.HasValue
            && candidate.ParentCategoryId.Value == referenceParentCategoryId.Value)
            return 0.50;

        return 0.00;
    }

    private static double ConditionSimilarityScore(
        ProductCondition? candidateCondition,
        ProductCondition? referenceCondition)
    {
        if (candidateCondition is null || referenceCondition is null)
            return 0.00;

        if (candidateCondition == referenceCondition)
            return 1.00;

        bool adjacent =
            (candidateCondition == ProductCondition.New && referenceCondition == ProductCondition.LikeNew) ||
            (candidateCondition == ProductCondition.LikeNew && referenceCondition == ProductCondition.New) ||
            (candidateCondition == ProductCondition.LikeNew && referenceCondition == ProductCondition.Used) ||
            (candidateCondition == ProductCondition.Used && referenceCondition == ProductCondition.LikeNew);

        return adjacent ? 0.50 : 0.00;
    }

    private static double TitleKeywordScore(string candidateTitle, string referenceTitle)
    {
        if (string.IsNullOrWhiteSpace(candidateTitle) || string.IsNullOrWhiteSpace(referenceTitle))
            return 0.00;

        var candidateTokens = Tokenise(candidateTitle);
        var referenceTokens = Tokenise(referenceTitle);

        if (referenceTokens.Count == 0) return 0.00;

        var overlap = candidateTokens.Intersect(referenceTokens).Count();
        var score = (double)overlap / referenceTokens.Count;

        return Math.Min(score * 2.0, 1.0);
    }

    private static HashSet<string> Tokenise(string text)
    {
        return text
            .ToLowerInvariant()
            .Split([' ', ',', '.', '-', '_', '(', ')', '/', '\\'], StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length > 2)
            .ToHashSet();
    }
}