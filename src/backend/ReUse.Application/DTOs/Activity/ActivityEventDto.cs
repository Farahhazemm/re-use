using System;

namespace ReUse.Application.DTOs;

public class ActivityEventDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ProductId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; }
    public ProductBriefDto? Product { get; set; }
}