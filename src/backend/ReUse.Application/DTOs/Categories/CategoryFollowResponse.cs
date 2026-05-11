namespace ReUse.Application.DTOs.Categories;

public record CategoryFollowResponse
{
    public Guid CategoryId { get; init; }

    public string Name { get; init; } = default!;

    public string Slug { get; init; } = default!;

    public string? IconUrl { get; init; }

    public DateTime FollowedAt { get; init; }
}