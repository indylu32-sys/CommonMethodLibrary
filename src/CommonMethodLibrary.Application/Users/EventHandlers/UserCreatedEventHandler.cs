using CommonMethodLibrary.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CommonMethodLibrary.Application.Users.EventHandlers;

/// <summary>
/// 用户创建事件处理器 - 处理用户创建后的业务逻辑
/// 例如：发送欢迎邮件、创建默认设置等
/// </summary>
public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "用户创建事件触发: UserId={UserId}, Username={Username}, Email={Email}, EventId={EventId}",
            notification.UserId,
            notification.Username,
            notification.Email,
            notification.EventId
        );

        // 这里可以执行：
        // 1. 发送欢迎邮件
        // 2. 创建用户默认设置
        // 3. 发送到消息队列进行异步处理
        // 4. 记录审计日志
        // 5. 触发其他业务流程

        // 示例：模拟发送欢迎邮件
        await SendWelcomeEmailAsync(notification.Email, notification.Username, cancellationToken);
    }

    private async Task SendWelcomeEmailAsync(string email, string username, CancellationToken cancellationToken)
    {
        // 模拟发送邮件
        await Task.Delay(100, cancellationToken);
        _logger.LogInformation("欢迎邮件已发送到: {Email}", email);
    }
}
