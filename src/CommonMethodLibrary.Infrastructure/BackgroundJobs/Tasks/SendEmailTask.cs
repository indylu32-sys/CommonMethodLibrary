using Microsoft.Extensions.Logging;

namespace CommonMethodLibrary.Infrastructure.BackgroundJobs.Tasks;

/// <summary>
/// 发送邮件任务 - 示例后台任务
/// </summary>
public class SendEmailTask
{
    private readonly ILogger<SendEmailTask> _logger;

    public SendEmailTask(ILogger<SendEmailTask> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 执行发送邮件任务
    /// </summary>
    public async ValueTask ExecuteAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        _logger.LogInformation("开始发送邮件: To={To}, Subject={Subject}", to, subject);

        try
        {
            // 模拟发送邮件
            await Task.Delay(2000, cancellationToken);

            _logger.LogInformation("邮件发送成功: To={To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送邮件失败: To={To}", to);
            throw;
        }
    }
}

/// <summary>
/// 数据处理任务 - 示例后台任务
/// </summary>
public class DataProcessingTask
{
    private readonly ILogger<DataProcessingTask> _logger;

    public DataProcessingTask(ILogger<DataProcessingTask> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 执行数据处理任务
    /// </summary>
    public async ValueTask ExecuteAsync(string dataId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("开始处理数据: DataId={DataId}", dataId);

        try
        {
            // 模拟数据处理
            await Task.Delay(3000, cancellationToken);

            _logger.LogInformation("数据处理完成: DataId={DataId}", dataId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据处理失败: DataId={DataId}", dataId);
            throw;
        }
    }
}
