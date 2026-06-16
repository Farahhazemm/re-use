using AutoMapper;

using Microsoft.Extensions.Logging;

using ReUse.Application.DTOs;
using ReUse.Application.DTOs.SystemActivityLog;
using ReUse.Application.Exceptions;
using ReUse.Application.Interfaces;
using ReUse.Application.Interfaces.Services;
using ReUse.Domain.Entities;
using ReUse.Domain.Enums;

namespace ReUse.Application.Services;

public class SystemActivityLogService : ISystemActivityLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SystemActivityLogService> _logger;

    public SystemActivityLogService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SystemActivityLogService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }


    public async Task<PagedResult<SystemActivityLogResponse>> GetAllAsync(SystemActivityLogFilterParams filterParams)
        => await _unitOfWork.SystemActivityLogs.GetAllAsync(filterParams);

    public async Task<SystemActivityLogResponse> GetByIdAsync(Guid id)
    {
        var log = await _unitOfWork.SystemActivityLogs.GetByIdDetailAsync(id);
        if (log is null)
            throw new NotFoundException("SystemActivityLog");
        return _mapper.Map<SystemActivityLogResponse>(log);
    }


    public async Task LogAsync(CreateSystemActivityLogRequest request)
    {
        var safeDescription = SanitizeDescription(request.Description);
        var safeMetadata = SanitizeMetadata(request.Metadata);
        try
        {
            var entity = new SystemActivityLog
            {
                ActorUserId = request.ActorUserId,
                ActorName = request.ActorName,
                ActorEmail = request.ActorEmail,
                ActionType = request.ActionType,
                Category = request.Category,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                Severity = request.Severity,
                Status = request.Status,
                Description = safeDescription,
                IpAddress = request.IpAddress,
                UserAgent = TruncateUserAgent(request.UserAgent),
                Metadata = safeMetadata,
            };

            _unitOfWork.SystemActivityLogs.Add(entity);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to persist SystemActivityLog: Action={ActionType} Category={Category} Actor={ActorUserId}",
                request.ActionType, request.Category, request.ActorUserId);
        }
    }

    private static string SanitizeDescription(string? description)
    {
        var input = description ?? string.Empty;
        var redacted = SanitizeMetadata(input) ?? string.Empty;
        return Truncate(redacted, 1000);
    }

    public async Task LogLoginSuccessAsync(Guid userId, string? ipAddress = null, string? userAgent = null)
    {
        var (name, email) = await ResolveActorAsync(userId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = userId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.Login,
            Category = LogCategory.Authentication,
            Severity = LogSeverity.Info,
            Status = LogStatus.Success,
            Description = "User logged in successfully.",
            IpAddress = ipAddress,
            UserAgent = userAgent,
        });
    }

    public Task LogLoginFailedAsync(string email, string? ipAddress = null, string? userAgent = null, string? reason = null)
        => LogAsync(new CreateSystemActivityLogRequest
        {
            ActionType = LogActionType.LoginFailed,
            Category = LogCategory.Authentication,
            Severity = LogSeverity.Warning,
            Status = LogStatus.Failure,
            Description = $"Login attempt failed for email '{MaskEmail(email)}'. {reason}".TrimEnd(),
            IpAddress = ipAddress,
            UserAgent = userAgent,
        });

    public async Task LogPasswordChangedAsync(Guid userId, string? ipAddress = null, string? userAgent = null)
    {
        var (name, email) = await ResolveActorAsync(userId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = userId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.PasswordChanged,
            Category = LogCategory.Authentication,
            Severity = LogSeverity.Info,
            Status = LogStatus.Success,
            Description = "User changed their password.",
            IpAddress = ipAddress,
            UserAgent = userAgent,
        });
    }

    public Task LogPasswordResetAsync(string email, string? ipAddress = null, string? userAgent = null)
        => LogAsync(new CreateSystemActivityLogRequest
        {
            ActionType = LogActionType.PasswordReset,
            Category = LogCategory.Authentication,
            Severity = LogSeverity.Info,
            Status = LogStatus.Success,
            Description = $"Password reset completed for email '{MaskEmail(email)}'.",
            IpAddress = ipAddress,
            UserAgent = userAgent,
        });

    public Task LogAccountDeletedAsync(Guid userId, string actorEmail, string actorName, string? ipAddress = null, string? userAgent = null)
        => LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = userId,
            ActorName = actorName,
            ActorEmail = MaskEmail(actorEmail),
            ActionType = LogActionType.UserDeleted,
            Category = LogCategory.UserManagement,
            EntityType = "User",
            EntityId = userId.ToString(),
            Severity = LogSeverity.Warning,
            Status = LogStatus.Success,
            Description = $"User deleted their own account. Id='{userId}' Name='{actorName}' Email='{MaskEmail(actorEmail)}'.",
            IpAddress = ipAddress,
            UserAgent = userAgent,
        });

    public Task LogUnauthorizedAccessAsync(string? ipAddress = null, string? userAgent = null, string? path = null)
        => LogAsync(new CreateSystemActivityLogRequest
        {
            ActionType = LogActionType.UnauthorizedAccess,
            Category = LogCategory.Security,
            Severity = LogSeverity.Warning,
            Status = LogStatus.Failure,
            Description = $"Unauthorized access attempt{(path is null ? "." : $" to '{path}'.")}",
            IpAddress = ipAddress,
            UserAgent = userAgent,
        });

    public Task LogPermissionDeniedAsync(Guid? userId, string? ipAddress = null, string? userAgent = null, string? path = null)
        => LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = userId,
            ActionType = LogActionType.PermissionDenied,
            Category = LogCategory.Security,
            Severity = LogSeverity.Warning,
            Status = LogStatus.Failure,
            Description = $"Permission denied{(path is null ? "." : $" for '{path}'.")}",
            IpAddress = ipAddress,
            UserAgent = userAgent,
        });

    public async Task LogUserBlockedAsync(Guid actorAdminId, Guid targetUserId)
    {
        var (name, email) = await ResolveActorAsync(actorAdminId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = actorAdminId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.UserDeactivated,
            Category = LogCategory.UserManagement,
            EntityType = "User",
            EntityId = targetUserId.ToString(),
            Severity = LogSeverity.Warning,
            Status = LogStatus.Success,
            Description = $"Admin blocked user '{targetUserId}'.",
        });
    }

    public async Task LogUserUnblockedAsync(Guid actorAdminId, Guid targetUserId)
    {
        var (name, email) = await ResolveActorAsync(actorAdminId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = actorAdminId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.UserReactivated,
            Category = LogCategory.UserManagement,
            EntityType = "User",
            EntityId = targetUserId.ToString(),
            Severity = LogSeverity.Info,
            Status = LogStatus.Success,
            Description = $"Admin unblocked user '{targetUserId}'.",
        });
    }

    public async Task LogCategoryCreatedAsync(Guid actorAdminId, Guid categoryId, string categoryName)
    {
        var (name, email) = await ResolveActorAsync(actorAdminId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = actorAdminId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.CategoryCreated,
            Category = LogCategory.SystemConfiguration,
            EntityType = "Category",
            EntityId = categoryId.ToString(),
            Severity = LogSeverity.Info,
            Status = LogStatus.Success,
            Description = $"Admin created category '{categoryName}'.",
        });
    }

    public async Task LogCategoryUpdatedAsync(Guid actorAdminId, Guid categoryId, string categoryName)
    {
        var (name, email) = await ResolveActorAsync(actorAdminId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = actorAdminId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.CategoryUpdated,
            Category = LogCategory.SystemConfiguration,
            EntityType = "Category",
            EntityId = categoryId.ToString(),
            Severity = LogSeverity.Info,
            Status = LogStatus.Success,
            Description = $"Admin updated category '{categoryName}'.",
        });
    }

    public async Task LogCategoryDeletedAsync(Guid actorAdminId, Guid categoryId, string categoryName)
    {
        var (name, email) = await ResolveActorAsync(actorAdminId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = actorAdminId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.CategoryDeleted,
            Category = LogCategory.SystemConfiguration,
            EntityType = "Category",
            EntityId = categoryId.ToString(),
            Severity = LogSeverity.Warning,
            Status = LogStatus.Success,
            Description = $"Admin deleted category '{categoryName}'.",
        });
    }

    public async Task LogProductModerationAsync(Guid actorAdminId, Guid productId, ProductStatus newStatus, string? reason = null)
    {
        var actionType = newStatus switch
        {
            ProductStatus.Active when reason != null && reason.Contains("Restored") => LogActionType.ProductRestored,
            ProductStatus.Active => LogActionType.ProductApproved,
            ProductStatus.Deleted => LogActionType.ProductDeleted,
            _ => LogActionType.ProductRejected,
        };

        var (name, email) = await ResolveActorAsync(actorAdminId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = actorAdminId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = actionType,
            Category = LogCategory.ContentModeration,
            EntityType = "Product",
            EntityId = productId.ToString(),
            Severity = newStatus == ProductStatus.Deleted ? LogSeverity.Warning : LogSeverity.Info,
            Status = LogStatus.Success,
            Description = $"Admin changed product '{productId}' status to '{newStatus}'{(reason is null ? "." : $": {reason}")}",
        });
    }

    public async Task LogProductDeletedByAdminAsync(Guid actorAdminId, Guid productId)
    {
        var (name, email) = await ResolveActorAsync(actorAdminId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = actorAdminId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.ProductDeleted,
            Category = LogCategory.ContentModeration,
            EntityType = "Product",
            EntityId = productId.ToString(),
            Severity = LogSeverity.Warning,
            Status = LogStatus.Success,
            Description = $"Admin deleted product '{productId}'.",
        });
    }

    public async Task LogBroadcastNotificationAsync(Guid actorAdminId, string summary)
    {
        var (name, email) = await ResolveActorAsync(actorAdminId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = actorAdminId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.Other,
            Category = LogCategory.General,
            Severity = LogSeverity.Info,
            Status = LogStatus.Success,
            Description = $"Admin sent broadcast notification: {summary}",
        });
    }

    public async Task LogPremiumGrantedByAdminAsync(Guid actorAdminId, Guid productId, int durationDays)
    {
        var (name, email) = await ResolveActorAsync(actorAdminId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = actorAdminId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.PremiumGranted,
            Category = LogCategory.ProductManagement,
            EntityType = "Product",
            EntityId = productId.ToString(),
            Severity = LogSeverity.Info,
            Status = LogStatus.Success,
            Description = $"Admin granted premium to product '{productId}' for {durationDays} days.",
        });
    }

    public async Task LogPremiumRemovedByAdminAsync(Guid actorAdminId, Guid productId)
    {
        var (name, email) = await ResolveActorAsync(actorAdminId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = actorAdminId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.PremiumRemoved,
            Category = LogCategory.ProductManagement,
            EntityType = "Product",
            EntityId = productId.ToString(),
            Severity = LogSeverity.Warning,
            Status = LogStatus.Success,
            Description = $"Admin removed premium from product '{productId}'.",
        });
    }

    public async Task LogPaymentSuccessAsync(Guid userId, string transactionId, decimal amount)
    {
        var (name, email) = await ResolveActorAsync(userId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = userId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.PaymentSuccess,
            Category = LogCategory.PaymentManagement,
            EntityType = "Payment",
            EntityId = transactionId,
            Severity = LogSeverity.Info,
            Status = LogStatus.Success,
            Description = $"Payment succeeded. Transaction='{transactionId}' Amount={amount / 100m:F2} EGP.",
        });
    }

    public async Task LogPaymentFailedAsync(Guid userId, string transactionId, decimal amount, string? reason = null)
    {
        var (name, email) = await ResolveActorAsync(userId);
        await LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = userId,
            ActorName = name,
            ActorEmail = email is null ? null : MaskEmail(email),
            ActionType = LogActionType.PaymentFailed,
            Category = LogCategory.PaymentManagement,
            EntityType = "Payment",
            EntityId = transactionId,
            Severity = LogSeverity.Warning,
            Status = LogStatus.Failure,
            Description = $"Payment failed. Transaction='{transactionId}' Amount={amount / 100m:F2} EGP.{(reason is null ? "" : $" Reason: {reason}")}",
        });
    }

    public Task LogUnhandledExceptionAsync(Exception ex, string? path = null, Guid? userId = null)
        => LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = userId,
            ActionType = LogActionType.UnhandledException,
            Category = LogCategory.General,
            Severity = LogSeverity.Error,
            Status = LogStatus.Failure,
            Description = $"Unhandled exception on '{path ?? "unknown"}': {ex.GetType().Name} — {Truncate(ex.Message, 300)}",
        });

    public Task LogInfrastructureFailureAsync(string component, string details, Guid? userId = null)
        => LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = userId,
            ActionType = LogActionType.InfrastructureFailure,
            Category = LogCategory.General,
            Severity = LogSeverity.Critical,
            Status = LogStatus.Failure,
            Description = $"Infrastructure failure in '{component}': {Truncate(details, 300)}",
        });

    public Task LogReportCreatedAsync(Guid reporterUserId, Guid reportId, ReportTargetType targetType, Guid targetId, ReportReason reason, string? actorName = null, string? actorEmail = null)
        => LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = reporterUserId,
            ActorName = actorName,
            ActorEmail = actorEmail is null ? null : MaskEmail(actorEmail),
            ActionType = LogActionType.ReportCreated,
            Category = LogCategory.ContentModeration,
            EntityType = "Report",
            EntityId = reportId.ToString(),
            Severity = LogSeverity.Warning,
            Status = LogStatus.Success,
            Description = $"User submitted a report on {targetType} '{targetId}'. Reason: {reason}.",
        });

    public Task LogReportReviewedAsync(Guid reviewerAdminId, Guid reportId, ReportStatus newStatus, ReportTargetType targetType, Guid targetId, string? actorName = null, string? actorEmail = null)
        => LogAsync(new CreateSystemActivityLogRequest
        {
            ActorUserId = reviewerAdminId,
            ActorName = actorName,
            ActorEmail = actorEmail is null ? null : MaskEmail(actorEmail),
            ActionType = LogActionType.ReportReviewed,
            Category = LogCategory.ContentModeration,
            EntityType = "Report",
            EntityId = reportId.ToString(),
            Severity = newStatus == ReportStatus.Resolved ? LogSeverity.Info : LogSeverity.Warning,
            Status = LogStatus.Success,
            Description = $"Admin reviewed report '{reportId}' on {targetType} '{targetId}'. Decision: {newStatus}.",
        });

    private async Task<(string? name, string? email)> ResolveActorAsync(Guid userId)
    {
        var user = await _unitOfWork.User.GetByIdAsync(userId);
        return (user?.FullName, user?.Email);
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return "***";
        return email[0] + new string('*', Math.Min(at - 1, 3)) + email[at..];
    }

    private static string? TruncateUserAgent(string? ua)
        => ua is null ? null : Truncate(ua, 500);

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s[..max] + "…";

    private static string? SanitizeMetadata(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
            return metadata;

        var lower = metadata.ToLowerInvariant();
        if (lower.Contains("password") || lower.Contains("token") ||
            lower.Contains("otp") || lower.Contains("secret") ||
            lower.Contains("cvv") || lower.Contains("card_number"))
        {
            return "[REDACTED — sensitive data detected]";
        }

        return metadata;
    }
}