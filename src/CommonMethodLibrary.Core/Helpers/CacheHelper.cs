using System.Collections.Concurrent;

namespace CommonMethodLibrary.Core.Helpers;

/// <summary>
/// 内存缓存工具类
/// </summary>
public class CacheHelper
{
    private readonly ConcurrentDictionary<string, CacheItem> _cache = new();

    /// <summary>
    /// 设置缓存
    /// </summary>
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var item = new CacheItem
        {
            Value = value,
            ExpirationTime = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null
        };

        _cache.AddOrUpdate(key, item, (_, __) => item);
    }

    /// <summary>
    /// 获取缓存
    /// </summary>
    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var item))
        {
            if (item.ExpirationTime.HasValue && item.ExpirationTime.Value < DateTime.UtcNow)
            {
                // 缓存已过期，删除
                _cache.TryRemove(key, out _);
                return default;
            }

            return (T?)item.Value;
        }

        return default;
    }

    /// <summary>
    /// 尝试获取缓存
    /// </summary>
    public bool TryGet<T>(string key, out T? value)
    {
        value = Get<T>(key);
        return value != null;
    }

    /// <summary>
    /// 移除缓存
    /// </summary>
    public bool Remove(string key)
    {
        return _cache.TryRemove(key, out _);
    }

    /// <summary>
    /// 判断缓存是否存在
    /// </summary>
    public bool Exists(string key)
    {
        if (_cache.TryGetValue(key, out var item))
        {
            if (item.ExpirationTime.HasValue && item.ExpirationTime.Value < DateTime.UtcNow)
            {
                _cache.TryRemove(key, out _);
                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// 获取或设置缓存（如果不存在则创建）
    /// </summary>
    public T GetOrSet<T>(string key, Func<T> factory, TimeSpan? expiration = null)
    {
        if (TryGet<T>(key, out var value) && value != null)
        {
            return value;
        }

        var newValue = factory();
        Set(key, newValue, expiration);
        return newValue;
    }

    /// <summary>
    /// 获取或设置缓存（异步版本）
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (TryGet<T>(key, out var value) && value != null)
        {
            return value;
        }

        var newValue = await factory();
        Set(key, newValue, expiration);
        return newValue;
    }

    /// <summary>
    /// 获取所有缓存键
    /// </summary>
    public IEnumerable<string> GetAllKeys()
    {
        return _cache.Keys;
    }

    /// <summary>
    /// 清理过期缓存
    /// </summary>
    public void CleanupExpired()
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.ExpirationTime.HasValue && kvp.Value.ExpirationTime.Value < DateTime.UtcNow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }
    }

    private class CacheItem
    {
        public object? Value { get; set; }
        public DateTime? ExpirationTime { get; set; }
    }
}
