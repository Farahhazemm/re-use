using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Broadcast;
using ReUse.Application.DTOs.Notification.NotificationData;
using ReUse.Application.Exceptions;
using ReUse.Application.Interfaces;
using ReUse.Application.Interfaces.Services;
using ReUse.Domain.Entities;
using ReUse.Domain.Enums;
using ReUse.Infrastructure.Interfaces.Repositories;

namespace ReUse.Infrastructure.Services.Broadcast;

public class AdminBroadcastService : IAdminBroadcastService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationPublisher _publisher;
    private readonly IIdentityUserRepository _identityUserRepo;
    private readonly IActivityService _activityService;
    private readonly IMapper _mapper;
    private readonly ILogger<AdminBroadcastService> _logger;

    public AdminBroadcastService(
        IUnitOfWork unitOfWork,
        INotificationPublisher publisher,
        IIdentityUserRepository identityUserRepo,
        IActivityService activityService,
        IMapper mapper,
        ILogger<AdminBroadcastService> logger)
    {
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _identityUserRepo = identityUserRepo;
        _activityService = activityService;
        _mapper = mapper;
        _logger = logger;
    }

    #region Query

    public async Task<PagedResult<BroadcastResponse>> GetAllAsync(BroadcastFilterParams filterParams)
    {
        var paged = await _unitOfWork.Broadcasts.GetAllAsync(filterParams);
        return new PagedResult<BroadcastResponse>
        {
            Data = _mapper.Map<List<BroadcastResponse>>(paged.Data),
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalRecords = paged.TotalRecords
        };
    }

    public async Task<BroadcastResponse> GetByIdAsync(Guid id)
    {
        var broadcast = await _unitOfWork.Broadcasts.GetByIdWithCreatorAsync(id)
            ?? throw new NotFoundException("Broadcast");

        return _mapper.Map<BroadcastResponse>(broadcast);
    }

    public async Task<BroadcastSummaryStats> GetSummaryStatsAsync()
    {
        return await _unitOfWork.Broadcasts.GetSummaryStatsAsync();
    }

    #endregion

    #region Draft

    public async Task<BroadcastResponse> CreateDraftAsync(CreateBroadcastRequest request, Guid adminId)
    {
        _ = await _unitOfWork.User.GetByIdAsync(adminId)
            ?? throw new UnauthorizedException();

        var broadcast = new BroadcastMessage
        {
            Title = request.Title,
            Body = request.Body,
            TargetAudience = request.TargetAudience,
            Status = BroadcastStatus.Draft,
            ScheduledAt = request.ScheduledAt,
            CreatedByUserId = adminId
        };

        _unitOfWork.Broadcasts.Add(broadcast);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.CreateActivityAsync(
            adminId, null, "BroadcastDraftCreated",
            $"Draft broadcast created: {broadcast.Title}");

        var saved = await _unitOfWork.Broadcasts.GetByIdWithCreatorAsync(broadcast.Id)
            ?? throw new NotFoundException("Broadcast");

        return _mapper.Map<BroadcastResponse>(saved);
    }

    public async Task<BroadcastResponse> UpdateDraftAsync(Guid id, UpdateBroadcastRequest request, Guid adminId)
    {
        var broadcast = await _unitOfWork.Broadcasts.GetByIdWithCreatorAsync(id)
            ?? throw new NotFoundException("Broadcast");

        if (broadcast.Status != BroadcastStatus.Draft)
            throw new BadRequestException("Only drafts can be updated.");

        broadcast.Title = request.Title;
        broadcast.Body = request.Body;
        broadcast.TargetAudience = request.TargetAudience;
        broadcast.ScheduledAt = request.ScheduledAt;

        _unitOfWork.Broadcasts.Update(broadcast);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Broadcasts.GetByIdWithCreatorAsync(id)
            ?? throw new NotFoundException("Broadcast");

        return _mapper.Map<BroadcastResponse>(updated);
    }

    public async Task DeleteAsync(Guid id, Guid adminId)
    {
        var broadcast = await _unitOfWork.Broadcasts.GetByIdAsync(id)
            ?? throw new NotFoundException("Broadcast");

        if (broadcast.Status != BroadcastStatus.Draft)
            throw new BadRequestException("Only drafts can be deleted.");

        _unitOfWork.Broadcasts.Remove(broadcast);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.CreateActivityAsync(
            adminId, null, "BroadcastDraftDeleted",
            $"Draft broadcast deleted: {broadcast.Title}");
    }

    #endregion

    #region Send / Schedule

    public async Task<BroadcastResponse> SendAsync(CreateBroadcastRequest request, Guid adminId)
    {
        _ = await _unitOfWork.User.GetByIdAsync(adminId)
            ?? throw new UnauthorizedException();

        var broadcast = new BroadcastMessage
        {
            Title = request.Title,
            Body = request.Body,
            TargetAudience = request.TargetAudience,
            Status = BroadcastStatus.Draft,
            CreatedByUserId = adminId
        };

        _unitOfWork.Broadcasts.Add(broadcast);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.CreateActivityAsync(
            adminId, null, "BroadcastSentNow",
            $"Broadcast sent immediately: {broadcast.Title}");

        await ExecuteAsync(broadcast.Id);

        var result = await _unitOfWork.Broadcasts.GetByIdWithCreatorAsync(broadcast.Id)
            ?? throw new NotFoundException("Broadcast");

        return _mapper.Map<BroadcastResponse>(result);
    }

    public async Task<BroadcastResponse> ScheduleAsync(CreateBroadcastRequest request, Guid adminId)
    {
        _ = await _unitOfWork.User.GetByIdAsync(adminId)
            ?? throw new UnauthorizedException();

        if (!request.ScheduledAt.HasValue)
            throw new BadRequestException("ScheduledAt is required for scheduling.");

        var broadcast = new BroadcastMessage
        {
            Title = request.Title,
            Body = request.Body,
            TargetAudience = request.TargetAudience,
            Status = BroadcastStatus.Scheduled,
            ScheduledAt = request.ScheduledAt,
            CreatedByUserId = adminId
        };

        _unitOfWork.Broadcasts.Add(broadcast);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.CreateActivityAsync(
            adminId, null, "BroadcastScheduled",
            $"Broadcast scheduled for {request.ScheduledAt:u}: {broadcast.Title}");

        var saved = await _unitOfWork.Broadcasts.GetByIdWithCreatorAsync(broadcast.Id)
            ?? throw new NotFoundException("Broadcast");

        return _mapper.Map<BroadcastResponse>(saved);
    }

    #endregion

    #region Execution (shared path for Send Now and ScheduledBroadcastJob)

    public async Task ExecuteAsync(Guid broadcastId)
    {
        var broadcast = await _unitOfWork.Broadcasts.GetByIdAsync(broadcastId)
            ?? throw new NotFoundException("Broadcast");

        if (broadcast.SentAt.HasValue)
            return;

        broadcast.Status = BroadcastStatus.Processing;
        _unitOfWork.Broadcasts.Update(broadcast);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning(
                "Broadcast {BroadcastId} concurrency conflict while claiming Processing state — skipping",
                broadcastId);
            return;
        }

        var recipientIds = new List<Guid>();
        try
        {
            recipientIds = await ResolveRecipientsAsync(broadcast.TargetAudience);
            broadcast.RecipientCount = recipientIds.Count;

            var data = new BroadcastNotificationData { BroadcastId = broadcast.Id };

            await _publisher.PublishToMultipleAsync(
                recipientIds,
                NotificationType.AdminBroadcast,
                broadcast.Title,
                broadcast.Body,
                data);

            broadcast.Status = BroadcastStatus.Sent;
            broadcast.SentAt = DateTime.UtcNow;
            broadcast.DeliveredCount = recipientIds.Count;
            broadcast.FailedCount = 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Broadcast {BroadcastId} execution failed", broadcastId);
            broadcast.Status = BroadcastStatus.Failed;
            broadcast.FailedCount = recipientIds.Count;
            broadcast.DeliveredCount = 0;
        }

        _unitOfWork.Broadcasts.Update(broadcast);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region Private helpers

    private async Task<List<Guid>> ResolveRecipientsAsync(BroadcastAudience audience)
    {
        if (audience == BroadcastAudience.All)
            return await _unitOfWork.User.GetAllActiveUserIdsAsync();

        var roleName = audience switch
        {
            BroadcastAudience.Users => "User",
            BroadcastAudience.Admins => "Admin",
            _ => throw new BadRequestException("Unknown broadcast audience.")
        };

        var identityIds = await _identityUserRepo.GetUserIdsByRoleAsync(roleName);
        return await _unitOfWork.User.GetIdsByIdentityIdsAsync(identityIds);
    }

    #endregion
}