using ReUse.Application.DTOs;
using ReUse.Application.DTOs.SystemActivityLog;
using ReUse.Domain.Enums;

namespace ReUse.Application.Interfaces.Services;

public interface ISystemActivityLogService
{

    Task<PagedResult<SystemActivityLogResponse>> GetAllAsync(SystemActivityLogFilterParams filterParams);
    Task<SystemActivityLogResponse> GetByIdAsync(Guid id);


    Task LogAsync(CreateSystemActivityLogRequest request);

    Task LogLoginSuccessAsync(Guid userId, string? ipAddress = null, string? userAgent = null);
    Task LogLoginFailedAsync(string email, string? ipAddress = null, string? userAgent = null, string? reason = null);
    Task LogPasswordChangedAsync(Guid userId, string? ipAddress = null, string? userAgent = null);
    Task LogPasswordResetAsync(string email, string? ipAddress = null, string? userAgent = null);
    Task LogAccountDeletedAsync(Guid userId, string actorEmail, string actorName, string? ipAddress = null, string? userAgent = null);
    Task LogUnauthorizedAccessAsync(string? ipAddress = null, string? userAgent = null, string? path = null);
    Task LogPermissionDeniedAsync(Guid? userId, string? ipAddress = null, string? userAgent = null, string? path = null);

    Task LogUserBlockedAsync(Guid actorAdminId, Guid targetUserId);
    Task LogUserUnblockedAsync(Guid actorAdminId, Guid targetUserId);
    Task LogCategoryCreatedAsync(Guid actorAdminId, Guid categoryId, string categoryName);
    Task LogCategoryUpdatedAsync(Guid actorAdminId, Guid categoryId, string categoryName);
    Task LogCategoryDeletedAsync(Guid actorAdminId, Guid categoryId, string categoryName);
    Task LogProductModerationAsync(Guid actorAdminId, Guid productId, ProductStatus newStatus, string? reason = null);
    Task LogProductDeletedByAdminAsync(Guid actorAdminId, Guid productId);
    Task LogBroadcastNotificationAsync(Guid actorAdminId, string summary);
    Task LogPremiumGrantedByAdminAsync(Guid actorAdminId, Guid productId, int durationDays);
    Task LogPremiumRemovedByAdminAsync(Guid actorAdminId, Guid productId);

    Task LogPaymentSuccessAsync(Guid userId, string transactionId, decimal amount);
    Task LogPaymentFailedAsync(Guid userId, string transactionId, decimal amount, string? reason = null);

    Task LogUnhandledExceptionAsync(Exception ex, string? path = null, Guid? userId = null);
    Task LogInfrastructureFailureAsync(string component, string details, Guid? userId = null);

    Task LogReportCreatedAsync(Guid reporterUserId, Guid reportId, ReportTargetType targetType, Guid targetId, ReportReason reason, string? actorName = null, string? actorEmail = null);
    Task LogReportReviewedAsync(Guid reviewerAdminId, Guid reportId, ReportStatus newStatus, ReportTargetType targetType, Guid targetId, string? actorName = null, string? actorEmail = null);
}