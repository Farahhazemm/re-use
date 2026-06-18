using System;

using System.Collections.Generic;

using System.Threading.Tasks;

using ReUse.Domain.Entities;

namespace ReUse.Application.Interfaces.Repository;

public interface IActivityRepository : IBaseRepository<ActivityEvent>
{
    Task<List<ActivityEvent>> GetByUserIdAsync(Guid userId, int limit = 50);
    Task<(List<ActivityEvent> Items, bool HasMore)> GetHistoryAsync(
        Guid userId, int limit, DateTime? before, DateTime? from, DateTime? to, string? type);
}