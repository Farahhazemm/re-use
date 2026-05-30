using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.DTOs.Recommendations;

public record UserRecommendationContext
{
    public Guid? UserId { get; init; }

    public IReadOnlyList<Guid> FollowedCategoryIds { get; init; } = [];

    public IReadOnlyList<Guid> TopFavoritedCategoryIds { get; init; } = [];

    public IReadOnlyList<Guid> FollowingSellerIds { get; init; } = [];

    public string? UserCity { get; init; }

    public string? UserCountry { get; init; }
    public bool IsColdStart =>
        UserId is null ||
        (FollowedCategoryIds.Count == 0 &&
         TopFavoritedCategoryIds.Count == 0 &&
         FollowingSellerIds.Count == 0);
}