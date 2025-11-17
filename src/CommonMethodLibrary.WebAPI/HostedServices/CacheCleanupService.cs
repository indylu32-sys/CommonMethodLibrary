using CommonMethodLibrary.Core.Helpers;

namespace CommonMethodLibrary.WebAPI.HostedServices;

/// <summary>
/// 缓存清理后台服务
/// 演示如何在IHostedService中使用工具库
/// </summary>
public class CacheCleanupService : IHostedService, IDisposable
{
    private readonly ILogger<CacheCleanupService> _logger;
    private readonly CacheHelper _cache;
    private Timer? _timer;

    public CacheCleanupService(
        ILogger<CacheCleanupService> logger,
        CacheHelper cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("缓存清理服务启动于: {Time}", DateTimeHelper.ToIso8601(DateTime.Now));

        // 每5分钟清理一次过期缓存
        _timer = new Timer(
            DoWork,
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(5)
        );

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        try
        {
            _logger.LogInformation("开始清理过期缓存...");

            _cache.CleanupExpired();

            _logger.LogInformation("缓存清理完成，时间戳: {Timestamp}",
                DateTimeHelper.GetCurrentTimestamp());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理缓存时发生错误");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("缓存清理服务停止于: {Time}", DateTimeHelper.ToIso8601(DateTime.Now));

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
