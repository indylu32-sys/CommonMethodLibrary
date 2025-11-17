using CommonMethodLibrary.Core.Helpers;
using CommonMethodLibrary.WebAPI.Models.Common;
using System.Net;

namespace CommonMethodLibrary.WebAPI.Middleware;

/// <summary>
/// 全局异常处理中间件
/// 演示统一异常处理和工具库的使用
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorId = EncryptionHelper.GenerateShortGuid();

        _logger.LogError(exception, "未处理的异常 [ErrorId: {ErrorId}]: {Message}",
            errorId, exception.Message);

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = _environment.IsDevelopment()
                ? exception.Message
                : "服务器内部错误，请稍后重试",
            ErrorCode = errorId,
            Timestamp = DateTimeHelper.GetCurrentTimestamp()
        };

        // 在开发环境中添加详细错误信息
        if (_environment.IsDevelopment())
        {
            response.Data = new
            {
                Type = exception.GetType().Name,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException?.Message
            };
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await context.Response.WriteAsync(JsonHelper.Serialize(response));
    }
}
