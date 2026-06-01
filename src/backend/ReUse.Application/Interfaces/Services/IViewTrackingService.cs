using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.Interfaces.Services;

// Tracks product views with session-level deduplication.
// Calls are fire-and-forget; callers must not await the result
//when tracking a view during a GET request.
public interface IViewTrackingService
{
    Task TrackViewAsync(Guid productId, Guid? userId, string ipAddress, string userAgent);
}