using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Broadcast;

namespace ReUse.Application.Interfaces.Services;

public interface IAdminBroadcastService
{
    Task<PagedResult<BroadcastResponse>> GetAllAsync(BroadcastFilterParams filterParams);
    Task<BroadcastResponse> GetByIdAsync(Guid id);
    Task<BroadcastResponse> CreateDraftAsync(CreateBroadcastRequest request, Guid adminId);
    Task<BroadcastResponse> UpdateDraftAsync(Guid id, UpdateBroadcastRequest request, Guid adminId);
    Task<BroadcastResponse> SendAsync(CreateBroadcastRequest request, Guid adminId);
    Task<BroadcastResponse> ScheduleAsync(CreateBroadcastRequest request, Guid adminId);
    Task DeleteAsync(Guid id, Guid adminId);
    Task<BroadcastSummaryStats> GetSummaryStatsAsync();
    Task ExecuteAsync(Guid broadcastId);
}