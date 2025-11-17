using CommonMethodLibrary.WebAPI.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CommonMethodLibrary.WebAPI.Filters;

/// <summary>
/// 模型验证过滤器
/// 自动验证模型状态并返回统一的错误响应
/// </summary>
public class ModelValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "请求参数验证失败",
                ErrorCode = "VALIDATION_ERROR",
                Data = errors
            };

            context.Result = new BadRequestObjectResult(response);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // 不需要在Action执行后做任何操作
    }
}
