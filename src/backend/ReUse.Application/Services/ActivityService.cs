using AutoMapper;

using ReUse.Application.DTOs;

using ReUse.Application.DTOs.Activity;

using ReUse.Application.Exceptions;

using ReUse.Application.Interfaces;

using ReUse.Application.Interfaces.Services;

using ReUse.Domain.Entities;

namespace ReUse.Application.Services;

public class ActivityService : IActivityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ActivityService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ActivityEventDto?> GetActivityByIdAsync(Guid activityId)
    {
        var entity = await _unitOfWork.activities.GetByIdAsync(activityId);
        return entity is null ? null : _mapper.Map<ActivityEventDto>(entity);
    }

    public async Task<List<ActivityEventDto>> GetUserActivitiesAsync(Guid userId, int limit = 50)
    {
        if (userId == Guid.Empty)
            throw new BadRequestException("User ID cannot be empty.");
        if (limit <= 0 || limit > 1000)
            throw new BadRequestException("Limit must be between 1 and 1000.");

        var list = await _unitOfWork.activities.GetByUserIdAsync(userId, limit);
        return _mapper.Map<List<ActivityEventDto>>(list);
    }

    public async Task<ActivityHistoryResponse> GetUserActivityHistoryAsync(Guid userId, ActivityHistoryRequest request)
    {
        if (userId == Guid.Empty)
            throw new BadRequestException("User ID cannot be empty.");

        var limit = Math.Clamp(request.Limit, 1, 100);
        var (items, hasMore) = await _unitOfWork.activities.GetHistoryAsync(
            userId, limit, request.Before, request.From, request.To, request.Type);

        return new ActivityHistoryResponse
        {
            Items = _mapper.Map<List<ActivityEventDto>>(items),
            NextCursor = items.Count > 0 ? items[^1].Timestamp : null,
            HasMore = hasMore
        };
    }

    public async Task CreateActivityAsync(Guid userId, Guid? productId, string type, string? description = null, string? metadata = null)
    {
        var request = new CreateActivityRequest(userId, productId, type, description, metadata);
        var entity = _mapper.Map<ActivityEvent>(request);
        _unitOfWork.activities.Add(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}