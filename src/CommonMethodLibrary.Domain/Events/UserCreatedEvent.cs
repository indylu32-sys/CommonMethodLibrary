using CommonMethodLibrary.Domain.Common;

namespace CommonMethodLibrary.Domain.Events;

/// <summary>
/// 用户创建事件 - 当新用户被创建时触发
/// </summary>
public sealed class UserCreatedEvent : DomainEvent
{
    public int UserId { get; }
    public string Username { get; }
    public string Email { get; }

    public UserCreatedEvent(int userId, string username, string email)
    {
        UserId = userId;
        Username = username;
        Email = email;
    }
}

/// <summary>
/// 用户资料更新事件
/// </summary>
public sealed class UserProfileUpdatedEvent : DomainEvent
{
    public int UserId { get; }
    public string Username { get; }

    public UserProfileUpdatedEvent(int userId, string username)
    {
        UserId = userId;
        Username = username;
    }
}

/// <summary>
/// 用户激活事件
/// </summary>
public sealed class UserActivatedEvent : DomainEvent
{
    public int UserId { get; }
    public string Username { get; }

    public UserActivatedEvent(int userId, string username)
    {
        UserId = userId;
        Username = username;
    }
}

/// <summary>
/// 用户禁用事件
/// </summary>
public sealed class UserDeactivatedEvent : DomainEvent
{
    public int UserId { get; }
    public string Username { get; }

    public UserDeactivatedEvent(int userId, string username)
    {
        UserId = userId;
        Username = username;
    }
}

/// <summary>
/// 用户登录事件
/// </summary>
public sealed class UserLoggedInEvent : DomainEvent
{
    public int UserId { get; }
    public string Username { get; }
    public DateTime LoginTime { get; }

    public UserLoggedInEvent(int userId, string username, DateTime loginTime)
    {
        UserId = userId;
        Username = username;
        LoginTime = loginTime;
    }
}

/// <summary>
/// 用户密码更改事件
/// </summary>
public sealed class UserPasswordChangedEvent : DomainEvent
{
    public int UserId { get; }
    public string Username { get; }

    public UserPasswordChangedEvent(int userId, string username)
    {
        UserId = userId;
        Username = username;
    }
}
