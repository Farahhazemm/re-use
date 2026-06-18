using ReUse.Application.DTOs;
using ReUse.Application.DTOs.Broadcast;
using ReUse.Domain.Entities;

namespace ReUse.Application.Interfaces.Repository;

public interface IBroadcastRepository : IBaseRepository<BroadcastMessage>
{
    Task<PagedResult<BroadcastMessage>> GetAllAsync(BroadcastFilterParams filterParams);
    Task<BroadcastMessage?> GetByIdWithCreatorAsync(Guid id);
    Task<BroadcastSummaryStats> GetSummaryStatsAsync();
    Task<List<BroadcastMessage>> GetDueScheduledAsync(DateTime utcNow);
}