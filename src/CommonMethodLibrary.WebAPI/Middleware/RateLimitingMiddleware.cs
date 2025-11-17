using CommonMethodLibrary.Core.Helpers;
using CommonMethodLibrary.WebAPI.Models.Common;
using System.Collections.Concurrent;

namespace CommonMethodLibrary.WebAPI.Middleware;

/// <summary>
/// 简单的速率限制中间件
/// 演示如何使用CacheHelper实现速率限制
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, RequestCounter> _requestCounters = new();
    private readonly int _maxRequestsPerMinute;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        int maxRequestsPerMinute = 100)
    {
        _next = next;
        _logger = logger;
        _maxRequestsPerMinute = maxRequestsPerMinute;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var counter = _requestCounters.GetOrAdd(clientId, _ => new RequestCounter());

        lock (counter)
        {
            var now = DateTime.UtcNow;

            // 重置计数器（如果超过1分钟）
            if ((now - counter.WindowStart).TotalMinutes >= 1)
            {
                counter.Count = 0;
                counter.WindowStart = now;
            }

            counter.Count++;

            if (counter.Count > _maxRequestsPerMinute)
            {
                _logger.LogWarning("速率限制触发: {ClientId} - {Count} 请求/分钟",
                    clientId, counter.Count);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<object>.Fail(
                    "请求过于频繁，请稍后再试",
                    "RATE_LIMIT_EXCEEDED"
                );

                await context.Response.WriteAsync(JsonHelper.Serialize(response));
                return;
            }
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // 优先使用 X-Forwarded-For 头
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // 使用远程IP地址
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private class RequestCounter
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;
    }
}
