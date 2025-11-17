using CommonMethodLibrary.Core.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace CommonMethodLibrary.WebAPI.Filters;

/// <summary>
/// 性能日志过滤器
/// 记录每个Action的执行时间
/// </summary>
public class PerformanceLoggingFilter : IAsyncActionFilter
{
    private readonly ILogger<PerformanceLoggingFilter> _logger;

    public PerformanceLoggingFilter(ILogger<PerformanceLoggingFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var actionName = $"{context.Controller.GetType().Name}.{context.ActionDescriptor.DisplayName}";

        _logger.LogInformation("开始执行 Action: {ActionName}, 时间: {Timestamp}",
            actionName, DateTimeHelper.GetCurrentTimestamp());

        var result = await next();

        stopwatch.Stop();

        var logLevel = stopwatch.ElapsedMilliseconds > 1000 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(logLevel, "完成执行 Action: {ActionName}, 耗时: {ElapsedMs}ms",
            actionName, stopwatch.ElapsedMilliseconds);
    }
}
