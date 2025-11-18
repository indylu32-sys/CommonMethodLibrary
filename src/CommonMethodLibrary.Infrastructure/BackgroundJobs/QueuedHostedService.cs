using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommonMethodLibrary.Infrastructure.BackgroundJobs;

/// <summary>
/// 队列后台服务 - 处理队列中的任务
/// </summary>
public class QueuedHostedService : BackgroundService
{
    private readonly ILogger<QueuedHostedService> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("队列后台服务启动");

        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _taskQueue.DequeueAsync(stoppingToken);

            try
            {
                _logger.LogInformation("开始执行队列任务");
                await workItem(stoppingToken);
                _logger.LogInformation("队列任务执行完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行队列任务时发生错误");
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("队列后台服务停止");
        await base.StopAsync(stoppingToken);
    }
}
