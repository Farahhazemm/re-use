using System;

namespace ReUse.Application.DTOs;

public class ProductBriefDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Condition { get; set; }
    public string? LocationCity { get; set; }
    public string? LocationCountry { get; set; }
    public string SellerName { get; set; } = string.Empty;
}