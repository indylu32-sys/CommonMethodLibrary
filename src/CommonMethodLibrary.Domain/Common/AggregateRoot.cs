namespace CommonMethodLibrary.Domain.Common;

/// <summary>
/// 聚合根基类 - DDD聚合根的基础抽象
/// 聚合根是一组相关实体的入口点，负责维护聚合内的一致性
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    /// <summary>
    /// 版本号 - 用于乐观并发控制
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// 增加版本号
    /// </summary>
    protected void IncrementVersion()
    {
        Version++;
    }
}
