using ReUse.Application.Enums;
using ReUse.Domain.Enums;

namespace ReUse.Application.DTOs.Recommendations;

public record CandidateProduct
{
    public Guid Id { get; init; }

    public Guid CategoryId { get; init; }

    public Guid? ParentCategoryId { get; init; }

    public Guid OwnerUserId { get; init; }

    public string Title { get; init; } = string.Empty;

    public ProductCondition? Condition { get; init; }

    public string? LocationCity { get; init; }

    public string? LocationCountry { get; init; }

    public DateTime CreatedAt { get; init; }


    public int RecentFavoriteCount { get; init; }

    public int CommentCount { get; init; }

    public int ViewCount { get; init; }

    public bool IsPremium { get; init; }

    public DateTime? PremiumExpiresAt { get; init; }

    public CandidateBucket Bucket { get; init; }
}