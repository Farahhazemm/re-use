using System;
using System.Collections.Generic;

namespace ReUse.Application.DTOs.Activity;

public class ActivityHistoryResponse
{
    public List<ActivityEventDto> Items { get; set; } = new();
    public DateTime? NextCursor { get; set; }
    public bool HasMore { get; set; }
}