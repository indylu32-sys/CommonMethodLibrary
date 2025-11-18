using CommonMethodLibrary.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CommonMethodLibrary.Application.Users.EventHandlers;

/// <summary>
/// 用户登录事件处理器
/// </summary>
public class UserLoggedInEventHandler : INotificationHandler<UserLoggedInEvent>
{
    private readonly ILogger<UserLoggedInEventHandler> _logger;

    public UserLoggedInEventHandler(ILogger<UserLoggedInEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "用户登录事件: UserId={UserId}, Username={Username}, LoginTime={LoginTime}",
            notification.UserId,
            notification.Username,
            notification.LoginTime
        );

        // 这里可以：
        // 1. 记录登录日志
        // 2. 检测异常登录（如不同地点登录）
        // 3. 更新用户活跃度
        // 4. 发送登录通知

        await Task.CompletedTask;
    }
}
