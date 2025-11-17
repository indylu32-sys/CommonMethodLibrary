using CommonMethodLibrary.Core.Helpers;
using System.Diagnostics;

namespace CommonMethodLibrary.WebAPI.Middleware;

/// <summary>
/// 请求日志中间件
/// 演示如何在中间件中使用工具库
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = EncryptionHelper.GenerateShortGuid();
        var stopwatch = Stopwatch.StartNew();

        // 记录请求信息
        var requestInfo = new
        {
            RequestId = requestId,
            Method = context.Request.Method,
            Path = context.Request.Path,
            QueryString = context.Request.QueryString.ToString(),
            Timestamp = DateTimeHelper.GetCurrentTimestamp(),
            UserAgent = context.Request.Headers["User-Agent"].ToString()
        };

        _logger.LogInformation("请求开始: {RequestInfo}", JsonHelper.Serialize(requestInfo));

        // 添加请求ID到响应头
        context.Response.Headers.Add("X-Request-Id", requestId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // 记录响应信息
            var responseInfo = new
            {
                RequestId = requestId,
                StatusCode = context.Response.StatusCode,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Timestamp = DateTimeHelper.GetCurrentTimestamp()
            };

            if (context.Response.StatusCode >= 400)
            {
                _logger.LogWarning("请求完成(错误): {ResponseInfo}", JsonHelper.Serialize(responseInfo));
            }
            else
            {
                _logger.LogInformation("请求完成: {ResponseInfo}", JsonHelper.Serialize(responseInfo));
            }
        }
    }
}
