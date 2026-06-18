using System;

namespace ReUse.Application.DTOs.Activity;

public class ActivityHistoryRequest
{
    public int Limit { get; set; } = 20;
    public DateTime? Before { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Type { get; set; }
}