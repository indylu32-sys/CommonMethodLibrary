using CommonMethodLibrary.Core.Helpers;
using CommonMethodLibrary.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CommonMethodLibrary.WebAPI.HostedServices;

/// <summary>
/// 数据种子后台服务
/// 在应用启动时初始化数据库
/// </summary>
public class DataSeedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataSeedService> _logger;

    public DataSeedService(
        IServiceProvider serviceProvider,
        ILogger<DataSeedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("数据初始化服务启动于: {Time}", DateTimeHelper.ToIso8601(DateTime.Now));

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            // 确保数据库已创建
            await context.Database.EnsureCreatedAsync(cancellationToken);

            _logger.LogInformation("数据库初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据库初始化失败");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("数据初始化服务停止");
        return Task.CompletedTask;
    }
}
