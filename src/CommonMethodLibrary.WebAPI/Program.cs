using CommonMethodLibrary.Core.Helpers;
using CommonMethodLibrary.WebAPI.Data;
using CommonMethodLibrary.WebAPI.Filters;
using CommonMethodLibrary.WebAPI.HostedServices;
using CommonMethodLibrary.WebAPI.Middleware;
using CommonMethodLibrary.WebAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 配置Serilog日志
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 添加数据库上下文（使用内存数据库）
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("CommonMethodLibraryDb"));

// 注册工具类为单例服务
builder.Services.AddSingleton<CacheHelper>();
builder.Services.AddHttpClient<HttpHelper>();

// 注册业务服务
builder.Services.AddScoped<IUserService, UserService>();

// 添加后台服务
builder.Services.AddHostedService<CacheCleanupService>();
builder.Services.AddHostedService<DataSeedService>();

// 添加控制器和过滤器
builder.Services.AddControllers(options =>
{
    // 添加全局过滤器
    options.Filters.Add<ModelValidationFilter>();
    options.Filters.Add<PerformanceLoggingFilter>();
});

// 配置CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 配置Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Common Method Library API",
        Version = "v1",
        Description = "一个完整的.NET Core演示项目，展示常用工具类库的使用方法",
        Contact = new OpenApiContact
        {
            Name = "示例项目",
            Email = "example@example.com"
        }
    });

    // 添加XML注释支持
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // 启用注解
    options.EnableAnnotations();
});

var app = builder.Build();

// 配置HTTP请求管道

// 使用全局异常处理中间件
app.UseMiddleware<GlobalExceptionMiddleware>();

// 使用请求日志中间件
app.UseMiddleware<RequestLoggingMiddleware>();

// 使用速率限制中间件
app.UseMiddleware<RateLimitingMiddleware>();

// 启用Swagger（所有环境）
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Common Method Library API V1");
    options.RoutePrefix = string.Empty; // 设置Swagger UI为根路径
    options.DocumentTitle = "Common Method Library API";
});

// 启用CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// 添加健康检查端点
app.MapGet("/health", () => new
{
    Status = "Healthy",
    Timestamp = DateTimeHelper.GetCurrentTimestamp(),
    Version = "1.0.0"
});

Log.Information("应用程序启动于: {Time}", DateTimeHelper.ToIso8601(DateTime.Now));

app.Run();
