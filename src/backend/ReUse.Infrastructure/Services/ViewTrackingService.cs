using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using ReUse.Application.Interfaces.Services;
using ReUse.Infrastructure.Persistence;

namespace ReUse.Infrastructure.Services;

public class ViewTrackingService : IViewTrackingService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ViewTrackingService> _logger;
    private static readonly TimeSpan DedupWindow = TimeSpan.FromMinutes(30);

    public ViewTrackingService(ApplicationDbContext context, IMemoryCache cache, ILogger<ViewTrackingService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task TrackViewAsync(Guid productId, Guid? userId, string ipAddress, string userAgent)
    {
        try
        {
            var sessionKey = ComputeSessionKey(productId, userId, ipAddress, userAgent);

            if (_cache.TryGetValue(sessionKey, out _))
                return;

            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE products SET \"ViewCount\" = \"ViewCount\" + 1 WHERE \"Id\" = {0}",
                productId);

            _cache.Set(sessionKey, true, DedupWindow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track view for product {ProductId}", productId);
        }
    }

    private static string ComputeSessionKey(Guid productId, Guid? userId, string ipAddress, string userAgent)
    {
        var raw = $"{(userId?.ToString() ?? ipAddress)}|{userAgent}|{productId}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return $"vt:{Convert.ToHexString(hash)}";
    }
}