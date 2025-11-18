using MediatR;

namespace CommonMethodLibrary.Domain.Common;

/// <summary>
/// 领域事件接口 - 表示领域中发生的重要业务事件
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// 事件发生时间
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// 事件ID
    /// </summary>
    Guid EventId { get; }
}

/// <summary>
/// 领域事件基类
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; }
    public Guid EventId { get; }

    protected DomainEvent()
    {
        OccurredOn = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }
}
