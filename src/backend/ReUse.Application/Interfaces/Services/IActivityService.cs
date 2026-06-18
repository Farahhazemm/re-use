using System;

using System.Collections.Generic;

using System.Threading.Tasks;

using ReUse.Application.DTOs;

using ReUse.Application.DTOs.Activity;

namespace ReUse.Application.Interfaces.Services;

public interface IActivityService
{
    Task<ActivityEventDto?> GetActivityByIdAsync(Guid activityId);
    Task<List<ActivityEventDto>> GetUserActivitiesAsync(Guid userId, int limit = 50);
    Task<ActivityHistoryResponse> GetUserActivityHistoryAsync(Guid userId, ActivityHistoryRequest request);
    Task CreateActivityAsync(Guid userId, Guid? productId, string type, string? description = null, string? metadata = null);
}