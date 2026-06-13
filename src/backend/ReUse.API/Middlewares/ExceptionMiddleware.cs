using System.Security.Claims;

using ReUse.API.Responses;
using ReUse.Application.Errors;
using ReUse.Application.Exceptions;
using ReUse.Application.Interfaces.Services;

namespace ReUse.API.Middlewares;

// TODO: Inject IRequestContext (IHttpContextAccessor wrapper) once available,
//       so IpAddress/UserAgent resolution is centralised rather than repeated here.

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            // Log 401/403 produced by the auth pipeline (no exception thrown)
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                await TryLogSecurityEventAsync(context, unauthorized: true);
            }
            else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                await TryLogSecurityEventAsync(context, unauthorized: false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred on {Path}", context.Request.Path);
            await TryLogUnhandledExceptionAsync(context, ex);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        ErrorResponse response;
        int statusCode;

        if (exception is AppException appException)
        {
            statusCode = appException.StatusCode;

            response = exception switch
            {
                IdentityOperationException identityEx => new ErrorResponse
                {
                    Code = identityEx.ErrorCode,
                    Message = identityEx.Message,
                    Errors = identityEx.Errors
                },
                _ => new ErrorResponse
                {
                    Code = appException.ErrorCode,
                    Message = appException.Message
                }
            };
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;

            response = new ErrorResponse
            {
                Code = ErrorsCode.InternalServer,
                Message = "Internal server error"
            };
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response);
    }

    private static async Task TryLogSecurityEventAsync(HttpContext context, bool unauthorized)
    {
        try
        {
            var logService = context.RequestServices
                .GetService<ISystemActivityLogService>();
            if (logService is null) return;

            var ip = GetIpAddress(context);
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var path = context.Request.Path.ToString();

            if (unauthorized)
                await logService.LogUnauthorizedAccessAsync(ip, userAgent, path);
            else
                await logService.LogPermissionDeniedAsync(TryGetUserId(context), ip, userAgent, path);
        }
        catch
        {
            // Security logging must never break the response pipeline.
        }
    }

    private static async Task TryLogUnhandledExceptionAsync(HttpContext context, Exception ex)
    {
        try
        {
            if (ex is AppException) return;

            var logService = context.RequestServices
                .GetService<ISystemActivityLogService>();
            if (logService is null) return;

            await logService.LogUnhandledExceptionAsync(
                ex,
                path: context.Request.Path.ToString(),
                userId: TryGetUserId(context));
        }
        catch
        {
            // Never let logging break the error pipeline.
        }
    }

    private static Guid? TryGetUserId(HttpContext context)
    {
        var value = context.User?.FindFirstValue("business_user_id");
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static string? GetIpAddress(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString();
    }
}