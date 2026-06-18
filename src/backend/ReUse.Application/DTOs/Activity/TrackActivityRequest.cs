using System;

namespace ReUse.Application.DTOs.Activity;

public class TrackActivityRequest
{
    public Guid? ProductId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Metadata { get; set; }
}