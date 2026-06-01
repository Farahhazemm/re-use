using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using ReUse.Application.Interfaces.Services;
using ReUse.Infrastructure.Persistence;

namespace ReUse.Infrastructure.Services;

public class ViewTrackingService : IViewTrackingService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DedupWindow = TimeSpan.FromMinutes(30);

    public ViewTrackingService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task TrackViewAsync(Guid productId, Guid? userId, string ipAddress, string userAgent)
    {
        var sessionKey = ComputeSessionKey(productId, userId, ipAddress, userAgent);

        // If already seen in the dedup window, skip silently
        if (_cache.TryGetValue(sessionKey, out _))
            return;

        // Increment via raw SQL — avoids EF change tracking and is safe under concurrency
        await _context.Database.ExecuteSqlRawAsync(
            "UPDATE products SET \"ViewCount\" = \"ViewCount\" + 1 WHERE \"Id\" = {0}",
            productId);

        _cache.Set(sessionKey, true, DedupWindow);
    }

    private static string ComputeSessionKey(Guid productId, Guid? userId, string ipAddress, string userAgent)
    {
        var raw = $"{(userId?.ToString() ?? ipAddress)}|{userAgent}|{productId}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return $"vt:{Convert.ToHexString(hash)}";
    }
}