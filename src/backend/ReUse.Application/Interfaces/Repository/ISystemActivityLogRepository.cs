using ReUse.Application.DTOs;
using ReUse.Application.DTOs.SystemActivityLog;
using ReUse.Domain.Entities;

namespace ReUse.Application.Interfaces.Repository;

public interface ISystemActivityLogRepository : IBaseRepository<SystemActivityLog>
{
    Task<PagedResult<SystemActivityLogResponse>> GetAllAsync(SystemActivityLogFilterParams filterParams);
    Task<SystemActivityLog?> GetByIdDetailAsync(Guid id);
}