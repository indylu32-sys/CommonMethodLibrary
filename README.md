# Common Method Library

一个完整的 .NET Core 8.0 + Vue 3 演示项目，展示了企业级应用的最佳实践和常用工具类库的使用方法。

## 🎯 项目简介

本项目是一个全栈演示应用，包含：
- ✅ ASP.NET Core Web API 后端
- ✅ Vue 3 + Vite + Element Plus 前端
- ✅ 完整的 RESTful CRUD 示例（用户管理）
- ✅ 丰富的工具类库演示
- ✅ Swagger API 文档
- ✅ 单元测试示例
- ✅ 中间件、过滤器、后台服务等最佳实践

## 🏗️ 项目结构

```
CommonMethodLibrary/
├── src/
│   ├── CommonMethodLibrary.Core/          # 核心工具类库
│   │   ├── Helpers/                       # 工具类
│   │   │   ├── StringHelper.cs           # 字符串处理
│   │   │   ├── DateTimeHelper.cs         # 日期时间处理
│   │   │   ├── ValidationHelper.cs       # 数据验证
│   │   │   ├── JsonHelper.cs             # JSON处理
│   │   │   ├── EncryptionHelper.cs       # 加密工具
│   │   │   ├── FileHelper.cs             # 文件处理
│   │   │   ├── HttpHelper.cs             # HTTP请求
│   │   │   └── CacheHelper.cs            # 缓存管理
│   │   └── Attributes/                    # 验证特性
│   │
│   ├── CommonMethodLibrary.WebAPI/        # Web API 项目
│   │   ├── Controllers/                   # 控制器
│   │   │   ├── UsersController.cs        # 用户管理API
│   │   │   └── ToolsDemoController.cs    # 工具演示API
│   │   ├── Models/                        # 模型
│   │   │   ├── Entities/                 # 实体类
│   │   │   ├── DTOs/                     # 数据传输对象
│   │   │   └── Common/                   # 通用模型
│   │   ├── Services/                      # 服务层
│   │   ├── Data/                          # 数据访问层
│   │   ├── Middleware/                    # 中间件
│   │   │   ├── RequestLoggingMiddleware.cs
│   │   │   ├── GlobalExceptionMiddleware.cs
│   │   │   └── RateLimitingMiddleware.cs
│   │   ├── Filters/                       # 过滤器
│   │   │   ├── ModelValidationFilter.cs
│   │   │   └── PerformanceLoggingFilter.cs
│   │   ├── HostedServices/                # 后台服务
│   │   │   ├── CacheCleanupService.cs
│   │   │   └── DataSeedService.cs
│   │   └── Program.cs                     # 启动配置
│   │
│   └── CommonMethodLibrary.Frontend/     # Vue3 前端项目
│       ├── src/
│       │   ├── views/                     # 页面组件
│       │   │   ├── Home.vue              # 首页
│       │   │   ├── Users.vue             # 用户管理
│       │   │   ├── Tools.vue             # 工具演示
│       │   │   └── About.vue             # 关于页面
│       │   ├── api/                       # API请求
│       │   │   ├── request.js            # Axios配置
│       │   │   ├── user.js               # 用户API
│       │   │   └── tools.js              # 工具API
│       │   ├── router/                    # 路由配置
│       │   ├── App.vue                    # 根组件
│       │   └── main.js                    # 入口文件
│       ├── index.html
│       ├── vite.config.js
│       └── package.json
│
├── tests/
│   └── CommonMethodLibrary.Tests/        # 单元测试项目
│       ├── Helpers/                       # 工具类测试
│       └── Services/                      # 服务层测试
│
└── CommonMethodLibrary.sln               # 解决方案文件
```

## 🚀 技术栈

### 后端技术
- **ASP.NET Core 8.0** - Web 框架
- **Entity Framework Core** - ORM 框架（内存数据库）
- **Serilog** - 日志框架
- **Swagger/OpenAPI** - API 文档
- **xUnit** - 单元测试框架
- **Moq** - Mock 框架

### 前端技术
- **Vue 3** - 渐进式 JavaScript 框架
- **Vite** - 下一代前端构建工具
- **Element Plus** - Vue 3 组件库
- **Vue Router** - 官方路由管理器
- **Pinia** - 状态管理库
- **Axios** - HTTP 客户端

## 📦 核心功能

### 工具类库

#### 1. StringHelper - 字符串处理
- 驼峰/帕斯卡/蛇形命名转换
- 字符串截断
- 随机字符串生成
- 敏感信息掩码
- Base64 编解码
- HTML标签移除

#### 2. DateTimeHelper - 日期时间处理
- Unix时间戳转换
- 友好时间描述
- 工作日判断
- 周/月起止日期
- 年龄计算
- ISO 8601 格式化

#### 3. ValidationHelper - 数据验证
- 邮箱验证
- 手机号验证（中国大陆）
- 身份证号验证
- URL验证
- IP地址验证
- 强密码验证
- 数字/字母验证

#### 4. JsonHelper - JSON处理
- 对象序列化/反序列化
- 深度克隆
- JSON格式验证
- 美化/压缩JSON

#### 5. EncryptionHelper - 加密工具
- MD5/SHA256/SHA512 哈希
- AES 加密/解密
- HMAC-SHA256 签名
- GUID 生成
- 随机密钥生成

#### 6. FileHelper - 文件处理
- 文件读写
- 文件复制/移动
- 文件大小格式化
- 目录操作

#### 7. HttpHelper - HTTP请求
- GET/POST/PUT/DELETE 请求
- JSON/Form 数据提交
- 文件下载

#### 8. CacheHelper - 缓存管理
- 内存缓存
- 过期时间设置
- 自动清理过期缓存

### 架构特性

#### 中间件（Middleware）
- **RequestLoggingMiddleware** - 请求日志记录
- **GlobalExceptionMiddleware** - 全局异常处理
- **RateLimitingMiddleware** - 速率限制

#### 过滤器（Filters）
- **ModelValidationFilter** - 模型验证
- **PerformanceLoggingFilter** - 性能日志

#### 后台服务（Hosted Services）
- **CacheCleanupService** - 定时清理过期缓存
- **DataSeedService** - 数据初始化

## 🛠️ 快速开始

### 前置要求

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)

### 安装步骤

1. **克隆项目**
```bash
git clone <repository-url>
cd CommonMethodLibrary
```

2. **运行后端API**
```bash
cd src/CommonMethodLibrary.WebAPI
dotnet restore
dotnet run
```

后端API将运行在：`http://localhost:5000`
Swagger文档：`http://localhost:5000`

3. **运行前端**
```bash
cd src/CommonMethodLibrary.Frontend
npm install
npm run dev
```

前端将运行在：`http://localhost:3000`

4. **运行测试**
```bash
cd tests/CommonMethodLibrary.Tests
dotnet test
```

## 📖 API 文档

启动后端服务后，访问 `http://localhost:5000` 查看 Swagger API 文档。

### 主要 API 端点

#### 用户管理
- `GET /api/users` - 获取所有用户
- `GET /api/users/paged` - 分页获取用户
- `GET /api/users/{id}` - 获取单个用户
- `POST /api/users` - 创建用户
- `PUT /api/users/{id}` - 更新用户
- `DELETE /api/users/{id}` - 删除用户

#### 工具演示
- `GET /api/ToolsDemo/string` - 字符串工具演示
- `GET /api/ToolsDemo/datetime` - 日期时间工具演示
- `POST /api/ToolsDemo/validation` - 验证工具演示
- `POST /api/ToolsDemo/json` - JSON工具演示
- `POST /api/ToolsDemo/encryption` - 加密工具演示
- `GET /api/ToolsDemo/file` - 文件工具演示
- `POST /api/ToolsDemo/cache/set` - 设置缓存
- `GET /api/ToolsDemo/cache/get/{key}` - 获取缓存

## 🧪 测试

项目包含完整的单元测试示例，展示如何测试：
- 工具类方法
- 服务层业务逻辑
- 使用 Mock 对象进行隔离测试

运行测试：
```bash
dotnet test
```

## 📝 代码示例

### 使用工具类库

```csharp
// 字符串处理
var camelCase = StringHelper.ToCamelCase("HelloWorld"); // "helloWorld"
var masked = StringHelper.MaskSensitiveInfo("13800138000", 3, 4); // "138****8000"

// 日期时间
var timestamp = DateTimeHelper.GetCurrentTimestamp();
var friendlyTime = DateTimeHelper.GetFriendlyTimeSpan(DateTime.Now.AddHours(-2)); // "2小时前"

// 数据验证
var isValidEmail = ValidationHelper.IsValidEmail("test@example.com");
var isStrongPassword = ValidationHelper.IsStrongPassword("Password123!");

// JSON处理
var json = JsonHelper.Serialize(myObject);
var obj = JsonHelper.Deserialize<MyClass>(json);

// 加密
var hash = EncryptionHelper.Sha256("mypassword");
var encrypted = EncryptionHelper.AesEncrypt(plainText, key, iv);

// 缓存
_cache.Set("key", value, TimeSpan.FromMinutes(5));
var cachedValue = _cache.Get<MyType>("key");
```

### 依赖注入示例

```csharp
// Program.cs
builder.Services.AddSingleton<CacheHelper>();
builder.Services.AddHttpClient<HttpHelper>();
builder.Services.AddScoped<IUserService, UserService>();

// 在控制器中使用
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
}
```

### 中间件使用示例

```csharp
// 使用工具类库的中间件
public class RequestLoggingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = EncryptionHelper.GenerateShortGuid();
        var timestamp = DateTimeHelper.GetCurrentTimestamp();

        _logger.LogInformation("Request: {RequestId} at {Timestamp}",
            requestId, timestamp);

        await _next(context);
    }
}
```

## 🎨 前端界面

前端提供了清晰的界面来演示所有功能：

1. **首页** - 项目介绍和快速导航
2. **用户管理** - 完整的CRUD操作界面
3. **工具演示** - 交互式演示所有工具类
4. **关于** - 项目文档和技术栈说明

## 📄 许可证

本项目仅供学习和演示使用。

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📞 联系方式

如有问题，请提交 Issue。

---

**注意**：本项目使用内存数据库，重启后数据会丢失。这是为了演示目的，实际生产环境应使用 SQL Server、PostgreSQL 等持久化数据库。
