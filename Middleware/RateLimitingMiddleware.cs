using System.Collections.Concurrent;

namespace Car_Project.Middleware
{
    /// <summary>
    /// Simple in-memory rate limiter for form submissions.
    /// Limits requests per IP address within a sliding time window.
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        // path prefix → (maxRequests, windowSeconds)
        private static readonly Dictionary<string, (int MaxRequests, int WindowSeconds)> _protectedPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            // Message sending (SignalR calls are separate; this covers POST endpoints)
            { "/Contact/SendMessage", (5, 60) },
            { "/Account/Register", (5, 300) },
            { "/Account/Login", (10, 300) },
        };

        // IP+path → list of request timestamps
        private static readonly ConcurrentDictionary<string, List<DateTime>> _requestLog = new();

        // Cleanup timer
        private static readonly Timer _cleanupTimer = new(_ =>
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-10);
            foreach (var key in _requestLog.Keys.ToList())
            {
                if (_requestLog.TryGetValue(key, out var timestamps))
                {
                    lock (timestamps)
                    {
                        timestamps.RemoveAll(t => t < cutoff);
                        if (timestamps.Count == 0)
                            _requestLog.TryRemove(key, out List<DateTime>? _);
                    }
                }
            }
        }, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == "POST")
            {
                var path = context.Request.Path.Value ?? "";

                foreach (var (protectedPath, limits) in _protectedPaths)
                {
                    if (path.StartsWith(protectedPath, StringComparison.OrdinalIgnoreCase))
                    {
                        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                        var key = $"{ip}:{protectedPath}";

                        var timestamps = _requestLog.GetOrAdd(key, _ => new List<DateTime>());
                        var now = DateTime.UtcNow;
                        var windowStart = now.AddSeconds(-limits.WindowSeconds);

                        bool rateLimited;
                        lock (timestamps)
                        {
                            timestamps.RemoveAll(t => t < windowStart);

                            if (timestamps.Count >= limits.MaxRequests)
                            {
                                rateLimited = true;
                            }
                            else
                            {
                                rateLimited = false;
                                timestamps.Add(now);
                            }
                        }

                        if (rateLimited)
                        {
                            _logger.LogWarning(
                                "Rate limit exceeded for IP {IP} on path {Path}. " +
                                "{Count} requests in {Window}s window.",
                                ip, protectedPath, limits.MaxRequests, limits.WindowSeconds);

                            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                            context.Response.ContentType = "text/html; charset=utf-8";
                            await context.Response.WriteAsync(
                                "<h2>Çox sayda sorğu göndərdiniz. Zəhmət olmasa bir az gözləyin.</h2>");
                            return;
                        }

                        break; // Only match one path
                    }
                }
            }

            await _next(context);
        }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
