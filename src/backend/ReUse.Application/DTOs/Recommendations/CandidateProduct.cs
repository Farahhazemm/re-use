using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    // Count of Favorites for this product. Used in PopularityScore.
    // NOTE: In V1 this is the all-time count derived at query time.
    // In V2 this will be replaced by the denormalised RecentFavoriteCount
    // (last 90 days) once that column is added to the Product table.

    public int FavoriteCount { get; init; }

    public int CommentCount { get; init; }

    public CandidateBucket Bucket { get; init; }
}