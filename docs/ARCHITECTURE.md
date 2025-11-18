# 架构设计文档

## 📐 整体架构

本项目采用 **清晰分层架构 (Clean Architecture)** + **DDD (领域驱动设计)** + **CQRS (命令查询职责分离)** + **事件驱动架构 (Event-Driven Architecture)**。

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│                   (WebAPI / Frontend)                        │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                    Application Layer                         │
│              (Commands, Queries, Handlers)                   │
│                    - CQRS 模式                               │
│                    - 应用服务                                │
│                    - 事件处理器                              │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                     Domain Layer                             │
│          (Entities, Value Objects, Domain Events)            │
│                    - 聚合根                                  │
│                    - 领域服务                                │
│                    - 仓储接口                                │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                  Infrastructure Layer                        │
│        (Repositories, EF Core, Event Bus, Jobs)             │
│                    - 数据持久化                              │
│                    - 外部服务集成                            │
│                    - 后台任务队列                            │
└─────────────────────────────────────────────────────────────┘
```

## 🏛️ 层次职责

### 1. Domain Layer (领域层) 🎯

**职责**: 核心业务逻辑和规则

**包含**:
- **实体 (Entity)**: 具有唯一标识的领域对象
- **聚合根 (Aggregate Root)**: 保证一致性边界的实体集合入口
- **值对象 (Value Object)**: 无标识的不可变对象
- **领域事件 (Domain Event)**: 表示领域中发生的重要事件
- **仓储接口 (Repository Interface)**: 定义数据访问契约
- **领域服务 (Domain Service)**: 不属于实体的业务逻辑

**示例**:
```csharp
// 聚合根
public class User : AggregateRoot<int>
{
    public static User Create(string username, string email, ...)
    {
        var user = new User(...);
        user.AddDomainEvent(new UserCreatedEvent(...));
        return user;
    }

    public void UpdateProfile(...)
    {
        // 业务规则验证
        MarkAsUpdated();
        AddDomainEvent(new UserProfileUpdatedEvent(...));
    }
}

// 值对象
public sealed class Email : ValueObject
{
    public static Email Create(string email)
    {
        if (!IsValid(email))
            throw new ArgumentException("邮箱格式不正确");
        return new Email(email);
    }
}
```

### 2. Application Layer (应用层) 📋

**职责**: 协调领域对象完成用户用例

**包含**:
- **Commands (命令)**: 写操作请求
- **Queries (查询)**: 读操作请求
- **Command Handlers**: 命令处理器
- **Query Handlers**: 查询处理器
- **DTOs**: 数据传输对象
- **Event Handlers**: 领域事件处理器
- **Application Services**: 应用服务

**CQRS 模式**:
```csharp
// 命令 - 写操作
public record CreateUserCommand(...) : ICommand<Result<UserDto>>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(...)
    {
        // 1. 验证
        // 2. 创建聚合根
        var user = User.Create(...);
        // 3. 保存
        await _repository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        // 4. 返回结果
        return Result<UserDto>.Success(dto);
    }
}

// 查询 - 读操作
public record GetUserByIdQuery(int Id) : IQuery<Result<UserDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(...)
    {
        var user = await _repository.GetByIdAsync(request.Id);
        return Result<UserDto>.Success(MapToDto(user));
    }
}
```

### 3. Infrastructure Layer (基础设施层) 🔧

**职责**: 实现技术细节和外部依赖

**包含**:
- **Repository 实现**: 数据访问实现
- **EF Core 配置**: 数据库映射
- **Event Bus**: 事件总线实现
- **Background Jobs**: 后台任务队列
- **外部服务集成**: 邮件、SMS等

**示例**:
```csharp
// 仓储实现
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public async Task<User?> GetByIdAsync(int id, ...)
    {
        return await _context.Users.FindAsync(id);
    }
}

// EF Core 配置
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // 配置值对象
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value)
            );
    }
}
```

### 4. Presentation Layer (表示层) 🖥️

**职责**: 处理用户交互和HTTP请求

**包含**:
- **Controllers**: API控制器
- **Middleware**: 中间件
- **Filters**: 过滤器
- **View Models**: 视图模型

## 🎭 DDD 核心概念

### 聚合根 (Aggregate Root)

聚合根是聚合的入口点，确保聚合内的一致性：

```csharp
public abstract class AggregateRoot<TId> : Entity<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
```

**特点**:
- ✅ 保证事务一致性边界
- ✅ 管理领域事件
- ✅ 封装业务规则
- ✅ 控制版本（乐观锁）

### 值对象 (Value Object)

值对象是不可变的，通过值相等：

```csharp
public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string phoneNumber)
    {
        if (!IsValid(phoneNumber))
            throw new ArgumentException("手机号格式不正确");
        return new PhoneNumber(phoneNumber);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**特点**:
- ✅ 不可变性
- ✅ 值相等（不是引用相等）
- ✅ 封装验证逻辑
- ✅ 自我验证

### 领域事件 (Domain Event)

领域事件表示业务中发生的重要事件：

```csharp
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

// 事件处理器
public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, ...)
    {
        // 发送欢迎邮件
        // 创建默认设置
        // 记录审计日志
    }
}
```

**优势**:
- ✅ 解耦业务逻辑
- ✅ 支持事件溯源
- ✅ 易于扩展
- ✅ 异步处理

## 🔄 CQRS 模式

命令查询职责分离 (CQRS) 将读写操作分离：

```
┌─────────────┐
│   Command   │ ──► 修改状态 ──► Repository ──► DB
└─────────────┘                                │
                                               │
┌─────────────┐                                │
│    Query    │ ──► 读取数据 ──────────────────┘
└─────────────┘
```

**优势**:
- ✅ 读写分离，优化性能
- ✅ 不同的模型适应不同的需求
- ✅ 简化复杂查询
- ✅ 支持读写扩展

## 📨 事件驱动架构

事件在保存实体时自动分发：

```csharp
public override async Task<int> SaveChangesAsync(...)
{
    // 分发领域事件
    await DispatchDomainEventsAsync();

    return await base.SaveChangesAsync();
}

private async Task DispatchDomainEventsAsync()
{
    var domainEvents = ChangeTracker
        .Entries<AggregateRoot<int>>()
        .SelectMany(x => x.Entity.DomainEvents)
        .ToList();

    foreach (var domainEvent in domainEvents)
    {
        await _mediator.Publish(domainEvent); // MediatR
    }
}
```

**流程**:
1. 用户执行命令 → 创建聚合根
2. 聚合根添加领域事件
3. SaveChanges时自动发布事件
4. 事件处理器异步处理

## 🔧 任务队列

后台任务队列用于异步处理耗时操作：

```csharp
// 入队
await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
{
    await _emailTask.ExecuteAsync(email, subject, body, token);
});

// 后台服务自动处理
public class QueuedHostedService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _taskQueue.DequeueAsync(stoppingToken);
            await workItem(stoppingToken);
        }
    }
}
```

**使用场景**:
- 📧 发送邮件/短信
- 📊 数据处理和分析
- 🖼️ 图片处理
- 📁 文件导入/导出
- 🔔 推送通知

## 📦 项目结构

```
src/
├── CommonMethodLibrary.Core/              # 工具类库
│   └── Helpers/                          # 各种Helper类
│
├── CommonMethodLibrary.Domain/           # 领域层 ⭐ DDD
│   ├── Common/                           # 基础抽象
│   │   ├── Entity.cs                    # 实体基类
│   │   ├── AggregateRoot.cs            # 聚合根基类
│   │   ├── ValueObject.cs              # 值对象基类
│   │   └── IDomainEvent.cs             # 领域事件接口
│   ├── Entities/                        # 实体
│   │   └── User.cs                     # 用户聚合根
│   ├── ValueObjects/                    # 值对象
│   │   ├── Email.cs
│   │   ├── PhoneNumber.cs
│   │   └── Address.cs
│   ├── Events/                          # 领域事件
│   │   └── UserCreatedEvent.cs
│   └── Repositories/                    # 仓储接口
│       ├── IRepository.cs
│       ├── IUserRepository.cs
│       └── IUnitOfWork.cs
│
├── CommonMethodLibrary.Application/      # 应用层 ⭐ CQRS
│   ├── Common/                           # 通用抽象
│   │   ├── ICommand.cs                  # 命令接口
│   │   ├── IQuery.cs                    # 查询接口
│   │   └── Result.cs                    # 结果包装
│   ├── Users/
│   │   ├── Commands/                    # 命令
│   │   │   ├── CreateUserCommand.cs
│   │   │   ├── UpdateUserCommand.cs
│   │   │   └── DeleteUserCommand.cs
│   │   ├── Queries/                     # 查询
│   │   │   ├── GetUserByIdQuery.cs
│   │   │   └── GetPagedUsersQuery.cs
│   │   ├── DTOs/                        # 数据传输对象
│   │   │   └── UserDto.cs
│   │   └── EventHandlers/               # 事件处理器
│   │       ├── UserCreatedEventHandler.cs
│   │       └── UserLoggedInEventHandler.cs
│
├── CommonMethodLibrary.Infrastructure/   # 基础设施层
│   ├── Persistence/                     # 数据持久化
│   │   ├── ApplicationDbContext.cs     # EF Core上下文
│   │   ├── Configurations/             # EF配置
│   │   │   └── UserConfiguration.cs
│   │   ├── Repositories/               # 仓储实现
│   │   │   └── UserRepository.cs
│   │   └── UnitOfWork.cs               # 工作单元
│   └── BackgroundJobs/                  # 后台任务 ⭐
│       ├── IBackgroundTaskQueue.cs
│       ├── BackgroundTaskQueue.cs      # 任务队列
│       ├── QueuedHostedService.cs      # 队列服务
│       └── Tasks/                       # 具体任务
│           └── SendEmailTask.cs
│
└── CommonMethodLibrary.WebAPI/          # 表示层
    ├── Controllers/                     # 控制器
    ├── Middleware/                      # 中间件
    └── Program.cs                       # 启动配置
```

## 🔑 核心优势

### 1. 清晰的职责分离
- 每层有明确的职责
- 依赖方向清晰（向内依赖）
- 易于理解和维护

### 2. 高度可测试
- 领域逻辑独立，易于单元测试
- 依赖注入，易于Mock
- CQRS分离，简化测试

### 3. 易于扩展
- 新功能添加新的Command/Query
- 事件驱动，松耦合
- 后台任务队列，异步处理

### 4. 领域驱动
- 业务逻辑集中在领域层
- 值对象封装验证
- 聚合根保证一致性

### 5. 事件驱动
- 解耦业务流程
- 支持异步处理
- 易于集成外部系统

## 🚀 使用示例

### 创建用户（完整流程）

```csharp
// 1. 客户端发送请求
POST /api/users
{
    "username": "john",
    "email": "john@example.com",
    "password": "Password123!"
}

// 2. Controller接收请求
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    var command = new CreateUserCommand(request.Username, request.Email, ...);
    var result = await _mediator.Send(command);
    return result.IsSuccess ? Ok(result) : BadRequest(result);
}

// 3. Command Handler处理命令
public class CreateUserCommandHandler
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, ...)
    {
        // 3.1 业务验证
        if (await _userRepository.UsernameExistsAsync(request.Username))
            return Result<UserDto>.Failure("用户名已存在");

        // 3.2 创建聚合根（领域层）
        var user = User.Create(request.Username, request.Email, ...);
        // user内部：AddDomainEvent(new UserCreatedEvent(...));

        // 3.3 持久化
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(); // 自动触发事件

        return Result<UserDto>.Success(MapToDto(user));
    }
}

// 4. 事件自动发布和处理
public class UserCreatedEventHandler
{
    public async Task Handle(UserCreatedEvent notification, ...)
    {
        // 4.1 发送欢迎邮件（加入队列）
        await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
        {
            await _emailTask.ExecuteAsync(
                notification.Email,
                "欢迎注册",
                "欢迎您...",
                token
            );
        });

        // 4.2 创建用户设置
        // 4.3 记录审计日志
    }
}

// 5. 后台队列处理邮件
// QueuedHostedService 自动从队列取出任务执行
```

## 📚 参考资料

- **Clean Architecture** by Robert C. Martin
- **Domain-Driven Design** by Eric Evans
- **CQRS** by Greg Young
- **MediatR** - .NET中的中介者模式实现
- **Event Sourcing** - 事件溯源模式

## 🎯 最佳实践

1. **聚合根不要过大** - 保持聚合边界小而清晰
2. **值对象不可变** - 确保值对象的不可变性
3. **事件命名过去式** - UserCreated而不是CreateUser
4. **命令清晰表意** - CreateUserCommand而不是UserCommand
5. **查询返回DTO** - 不要直接返回实体
6. **异步优先** - 使用async/await
7. **验证前置** - 在Command Handler中验证
8. **事件解耦** - 通过事件解耦业务流程
