# 企业级架构扩展指南

本文档详细说明如何基于现有的 DDD + CQRS + 事件驱动架构进行扩展，帮助开发者实现更复杂的企业级功能。

## 📑 目录

1. [扩展更多聚合根](#1-扩展更多聚合根)
2. [实现事件溯源](#2-实现事件溯源)
3. [添加消息队列](#3-添加消息队列)
4. [实现读写分离](#4-实现读写分离)
5. [添加微服务](#5-添加微服务)

---

## 1. 扩展更多聚合根

### 📍 在哪里添加

基于现有的 User 聚合根示例，新的聚合根遵循相同的结构：

```
src/
├── CommonMethodLibrary.Domain/
│   ├── Entities/
│   │   ├── User.cs                    # ✅ 现有示例
│   │   ├── Order.cs                   # 🆕 订单聚合根
│   │   ├── Product.cs                 # 🆕 产品聚合根
│   │   └── [YourAggregate].cs         # 🆕 您的聚合根
│   ├── ValueObjects/
│   │   ├── Email.cs                   # ✅ 现有示例
│   │   ├── OrderItem.cs               # 🆕 订单项值对象
│   │   ├── Money.cs                   # 🆕 金额值对象
│   │   └── [YourValueObject].cs       # 🆕 您的值对象
│   ├── Events/
│   │   ├── UserCreatedEvent.cs        # ✅ 现有示例
│   │   ├── OrderPlacedEvent.cs        # 🆕 订单创建事件
│   │   └── [YourEvent].cs             # 🆕 您的领域事件
│   └── Repositories/
│       ├── IUserRepository.cs         # ✅ 现有示例
│       ├── IOrderRepository.cs        # 🆕 订单仓储接口
│       └── [YourRepository].cs        # 🆕 您的仓储接口
```

### 🔨 怎么做

#### Step 1: 创建值对象

首先创建领域中的值对象（如金额、订单项等）：

**文件位置**: `src/CommonMethodLibrary.Domain/ValueObjects/Money.cs`

```csharp
using CommonMethodLibrary.Domain.Common;

namespace CommonMethodLibrary.Domain.ValueObjects;

/// <summary>
/// 金额值对象 - 封装货币金额和币种
/// </summary>
public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "CNY")
    {
        if (amount < 0)
            throw new ArgumentException("金额不能为负数", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("币种不能为空", nameof(currency));

        return new Money(amount, currency.ToUpper());
    }

    /// <summary>
    /// 加法运算
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("不同币种不能相加");

        return Create(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// 减法运算
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("不同币种不能相减");

        return Create(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// 乘法运算
    /// </summary>
    public Money Multiply(decimal multiplier)
    {
        return Create(Amount * multiplier, Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
```

**文件位置**: `src/CommonMethodLibrary.Domain/ValueObjects/OrderItem.cs`

```csharp
using CommonMethodLibrary.Domain.Common;

namespace CommonMethodLibrary.Domain.ValueObjects;

/// <summary>
/// 订单项值对象
/// </summary>
public class OrderItem : ValueObject
{
    public int ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money TotalPrice { get; private set; }

    private OrderItem(int productId, string productName, int quantity, Money unitPrice)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = unitPrice.Multiply(quantity);
    }

    public static OrderItem Create(int productId, string productName, int quantity, Money unitPrice)
    {
        if (productId <= 0)
            throw new ArgumentException("产品ID必须大于0", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("产品名称不能为空", nameof(productName));

        if (quantity <= 0)
            throw new ArgumentException("数量必须大于0", nameof(quantity));

        if (unitPrice == null)
            throw new ArgumentNullException(nameof(unitPrice));

        return new OrderItem(productId, productName, quantity, unitPrice);
    }

    /// <summary>
    /// 更新数量
    /// </summary>
    public OrderItem UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("数量必须大于0", nameof(newQuantity));

        return Create(ProductId, ProductName, newQuantity, UnitPrice);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ProductId;
        yield return ProductName;
        yield return Quantity;
        yield return UnitPrice;
    }
}
```

#### Step 2: 创建聚合根

**文件位置**: `src/CommonMethodLibrary.Domain/Entities/Order.cs`

```csharp
using CommonMethodLibrary.Domain.Common;
using CommonMethodLibrary.Domain.Events;
using CommonMethodLibrary.Domain.ValueObjects;

namespace CommonMethodLibrary.Domain.Entities;

/// <summary>
/// 订单聚合根
/// </summary>
public class Order : AggregateRoot<int>
{
    private readonly List<OrderItem> _items = new();

    public int UserId { get; private set; }
    public string OrderNumber { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public Money TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public Address? ShippingAddress { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? Remark { get; private set; }

    // EF Core 需要的无参构造函数
    private Order()
    {
        OrderNumber = string.Empty;
        TotalAmount = null!;
    }

    private Order(int userId, string orderNumber, Address shippingAddress)
    {
        UserId = userId;
        OrderNumber = orderNumber;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Pending;
        TotalAmount = Money.Create(0);
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 创建订单 - 工厂方法
    /// </summary>
    public static Order Create(int userId, Address shippingAddress, string? remark = null)
    {
        if (userId <= 0)
            throw new ArgumentException("用户ID必须大于0", nameof(userId));

        if (shippingAddress == null)
            throw new ArgumentNullException(nameof(shippingAddress));

        // 生成订单号 (格式: ORD-yyyyMMddHHmmss-随机4位)
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

        var order = new Order(userId, orderNumber, shippingAddress)
        {
            Remark = remark
        };

        // 发布订单创建事件
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, order.OrderNumber, userId));

        return order;
    }

    /// <summary>
    /// 添加订单项
    /// </summary>
    public void AddItem(int productId, string productName, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("只有待处理的订单可以添加商品");

        // 检查是否已存在该产品
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            // 更新数量
            _items.Remove(existingItem);
            _items.Add(existingItem.UpdateQuantity(existingItem.Quantity + quantity));
        }
        else
        {
            // 添加新商品
            var item = OrderItem.Create(productId, productName, quantity, unitPrice);
            _items.Add(item);
        }

        RecalculateTotalAmount();
        MarkAsUpdated();
        IncrementVersion();
    }

    /// <summary>
    /// 移除订单项
    /// </summary>
    public void RemoveItem(int productId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("只有待处理的订单可以移除商品");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotalAmount();
            MarkAsUpdated();
            IncrementVersion();
        }
    }

    /// <summary>
    /// 提交订单
    /// </summary>
    public void Submit()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("只有待处理的订单可以提交");

        if (!_items.Any())
            throw new InvalidOperationException("订单必须至少包含一个商品");

        Status = OrderStatus.Submitted;
        MarkAsUpdated();
        IncrementVersion();

        AddDomainEvent(new OrderSubmittedEvent(Id, OrderNumber, UserId, TotalAmount));
    }

    /// <summary>
    /// 支付订单
    /// </summary>
    public void Pay(string paymentMethod, string transactionId)
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOperationException("只有已提交的订单可以支付");

        Status = OrderStatus.Paid;
        PaidAt = DateTime.UtcNow;
        MarkAsUpdated();
        IncrementVersion();

        AddDomainEvent(new OrderPaidEvent(Id, OrderNumber, UserId, TotalAmount, paymentMethod, transactionId));
    }

    /// <summary>
    /// 发货
    /// </summary>
    public void Ship(string trackingNumber)
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException("只有已支付的订单可以发货");

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        MarkAsUpdated();
        IncrementVersion();

        AddDomainEvent(new OrderShippedEvent(Id, OrderNumber, trackingNumber));
    }

    /// <summary>
    /// 确认收货
    /// </summary>
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("只有已发货的订单可以确认收货");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        MarkAsUpdated();
        IncrementVersion();

        AddDomainEvent(new OrderDeliveredEvent(Id, OrderNumber, UserId));
    }

    /// <summary>
    /// 取消订单
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("已完成或已取消的订单不能再次取消");

        var previousStatus = Status;
        Status = OrderStatus.Cancelled;
        MarkAsUpdated();
        IncrementVersion();

        AddDomainEvent(new OrderCancelledEvent(Id, OrderNumber, UserId, reason, previousStatus));
    }

    /// <summary>
    /// 重新计算总金额
    /// </summary>
    private void RecalculateTotalAmount()
    {
        TotalAmount = _items.Aggregate(
            Money.Create(0),
            (sum, item) => sum.Add(item.TotalPrice)
        );
    }
}

/// <summary>
/// 订单状态枚举
/// </summary>
public enum OrderStatus
{
    Pending = 1,      // 待处理（购物车状态）
    Submitted = 2,    // 已提交（等待支付）
    Paid = 3,         // 已支付
    Shipped = 4,      // 已发货
    Delivered = 5,    // 已送达
    Cancelled = 6     // 已取消
}
```

**文件位置**: `src/CommonMethodLibrary.Domain/Entities/Product.cs`

```csharp
using CommonMethodLibrary.Domain.Common;
using CommonMethodLibrary.Domain.Events;
using CommonMethodLibrary.Domain.ValueObjects;

namespace CommonMethodLibrary.Domain.Entities;

/// <summary>
/// 产品聚合根
/// </summary>
public class Product : AggregateRoot<int>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Sku { get; private set; }
    public Money Price { get; private set; }
    public int StockQuantity { get; private set; }
    public int CategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public string? ImageUrl { get; private set; }
    public ProductStatus Status { get; private set; }

    // EF Core 需要的无参构造函数
    private Product()
    {
        Name = string.Empty;
        Description = string.Empty;
        Sku = string.Empty;
        Price = null!;
    }

    private Product(string name, string description, string sku, Money price, int stockQuantity, int categoryId)
    {
        Name = name;
        Description = description;
        Sku = sku;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        IsActive = true;
        Status = ProductStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 创建产品 - 工厂方法
    /// </summary>
    public static Product Create(string name, string description, string sku, Money price, int stockQuantity, int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("产品名称不能为空", nameof(name));

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU不能为空", nameof(sku));

        if (price == null)
            throw new ArgumentNullException(nameof(price));

        if (stockQuantity < 0)
            throw new ArgumentException("库存数量不能为负数", nameof(stockQuantity));

        if (categoryId <= 0)
            throw new ArgumentException("分类ID必须大于0", nameof(categoryId));

        var product = new Product(name, description, sku, price, stockQuantity, categoryId);

        product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Name, product.Sku));

        return product;
    }

    /// <summary>
    /// 更新产品信息
    /// </summary>
    public void UpdateInfo(string name, string description, Money price, int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("产品名称不能为空", nameof(name));

        Name = name;
        Description = description;
        Price = price ?? throw new ArgumentNullException(nameof(price));
        CategoryId = categoryId > 0 ? categoryId : throw new ArgumentException("分类ID必须大于0", nameof(categoryId));

        MarkAsUpdated();
        IncrementVersion();
    }

    /// <summary>
    /// 增加库存
    /// </summary>
    public void IncreaseStock(int quantity, string reason)
    {
        if (quantity <= 0)
            throw new ArgumentException("数量必须大于0", nameof(quantity));

        StockQuantity += quantity;
        MarkAsUpdated();
        IncrementVersion();

        AddDomainEvent(new ProductStockIncreasedEvent(Id, Name, quantity, reason));
    }

    /// <summary>
    /// 减少库存
    /// </summary>
    public void DecreaseStock(int quantity, string reason)
    {
        if (quantity <= 0)
            throw new ArgumentException("数量必须大于0", nameof(quantity));

        if (StockQuantity < quantity)
            throw new InvalidOperationException($"库存不足，当前库存: {StockQuantity}，需要: {quantity}");

        StockQuantity -= quantity;
        MarkAsUpdated();
        IncrementVersion();

        AddDomainEvent(new ProductStockDecreasedEvent(Id, Name, quantity, reason));

        // 库存预警
        if (StockQuantity < 10)
        {
            AddDomainEvent(new ProductLowStockEvent(Id, Name, StockQuantity));
        }
    }

    /// <summary>
    /// 上架产品
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        Status = ProductStatus.Active;
        MarkAsUpdated();
        IncrementVersion();

        AddDomainEvent(new ProductActivatedEvent(Id, Name));
    }

    /// <summary>
    /// 下架产品
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        Status = ProductStatus.Inactive;
        MarkAsUpdated();
        IncrementVersion();

        AddDomainEvent(new ProductDeactivatedEvent(Id, Name));
    }
}

/// <summary>
/// 产品状态枚举
/// </summary>
public enum ProductStatus
{
    Active = 1,      // 上架
    Inactive = 2,    // 下架
    OutOfStock = 3   // 缺货
}
```

#### Step 3: 创建领域事件

**文件位置**: `src/CommonMethodLibrary.Domain/Events/OrderEvents.cs`

```csharp
using CommonMethodLibrary.Domain.Common;
using CommonMethodLibrary.Domain.ValueObjects;
using CommonMethodLibrary.Domain.Entities;

namespace CommonMethodLibrary.Domain.Events;

// 订单创建事件
public record OrderCreatedEvent(int OrderId, string OrderNumber, int UserId) : IDomainEvent;

// 订单提交事件
public record OrderSubmittedEvent(int OrderId, string OrderNumber, int UserId, Money TotalAmount) : IDomainEvent;

// 订单支付事件
public record OrderPaidEvent(
    int OrderId,
    string OrderNumber,
    int UserId,
    Money TotalAmount,
    string PaymentMethod,
    string TransactionId
) : IDomainEvent;

// 订单发货事件
public record OrderShippedEvent(int OrderId, string OrderNumber, string TrackingNumber) : IDomainEvent;

// 订单送达事件
public record OrderDeliveredEvent(int OrderId, string OrderNumber, int UserId) : IDomainEvent;

// 订单取消事件
public record OrderCancelledEvent(
    int OrderId,
    string OrderNumber,
    int UserId,
    string Reason,
    OrderStatus PreviousStatus
) : IDomainEvent;
```

**文件位置**: `src/CommonMethodLibrary.Domain/Events/ProductEvents.cs`

```csharp
using CommonMethodLibrary.Domain.Common;

namespace CommonMethodLibrary.Domain.Events;

// 产品创建事件
public record ProductCreatedEvent(int ProductId, string Name, string Sku) : IDomainEvent;

// 库存增加事件
public record ProductStockIncreasedEvent(int ProductId, string Name, int Quantity, string Reason) : IDomainEvent;

// 库存减少事件
public record ProductStockDecreasedEvent(int ProductId, string Name, int Quantity, string Reason) : IDomainEvent;

// 低库存预警事件
public record ProductLowStockEvent(int ProductId, string Name, int CurrentStock) : IDomainEvent;

// 产品上架事件
public record ProductActivatedEvent(int ProductId, string Name) : IDomainEvent;

// 产品下架事件
public record ProductDeactivatedEvent(int ProductId, string Name) : IDomainEvent;
```

#### Step 4: 创建仓储接口

**文件位置**: `src/CommonMethodLibrary.Domain/Repositories/IOrderRepository.cs`

```csharp
using CommonMethodLibrary.Domain.Entities;

namespace CommonMethodLibrary.Domain.Repositories;

/// <summary>
/// 订单仓储接口
/// </summary>
public interface IOrderRepository : IRepository<Order, int>
{
    /// <summary>
    /// 根据订单号获取订单
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的订单列表
    /// </summary>
    Task<IEnumerable<Order>> GetUserOrdersAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定状态的订单
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
}
```

**文件位置**: `src/CommonMethodLibrary.Domain/Repositories/IProductRepository.cs`

```csharp
using CommonMethodLibrary.Domain.Entities;

namespace CommonMethodLibrary.Domain.Repositories;

/// <summary>
/// 产品仓储接口
/// </summary>
public interface IProductRepository : IRepository<Product, int>
{
    /// <summary>
    /// 根据SKU获取产品
    /// </summary>
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查SKU是否已存在
    /// </summary>
    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取分类下的产品
    /// </summary>
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取低库存产品
    /// </summary>
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default);
}
```

#### Step 5: 实现仓储

**文件位置**: `src/CommonMethodLibrary.Infrastructure/Persistence/Repositories/OrderRepository.cs`

```csharp
using CommonMethodLibrary.Domain.Entities;
using CommonMethodLibrary.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CommonMethodLibrary.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }

    public void Delete(Order order)
    {
        _context.Orders.Remove(order);
    }
}
```

#### Step 6: 创建 CQRS Commands 和 Queries

**文件位置**: `src/CommonMethodLibrary.Application/Orders/Commands/CreateOrderCommand.cs`

```csharp
using CommonMethodLibrary.Application.Common;
using CommonMethodLibrary.Domain.Entities;
using CommonMethodLibrary.Domain.Repositories;
using CommonMethodLibrary.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace CommonMethodLibrary.Application.Orders.Commands;

/// <summary>
/// 创建订单命令
/// </summary>
public record CreateOrderCommand(
    int UserId,
    AddressDto ShippingAddress,
    List<OrderItemDto> Items,
    string? Remark
) : ICommand<Result<int>>;

public record AddressDto(string Province, string City, string District, string Street, string? PostalCode);
public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice);

/// <summary>
/// 创建订单命令处理器
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<int>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 创建地址值对象
        var address = Address.Create(
            request.ShippingAddress.Province,
            request.ShippingAddress.City,
            request.ShippingAddress.District,
            request.ShippingAddress.Street,
            request.ShippingAddress.PostalCode
        );

        // 创建订单
        var order = Order.Create(request.UserId, address, request.Remark);

        // 添加订单项
        foreach (var item in request.Items)
        {
            // 验证产品是否存在
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
                return Result<int>.Failure($"产品不存在: {item.ProductId}");

            // 检查库存
            if (product.StockQuantity < item.Quantity)
                return Result<int>.Failure($"产品库存不足: {product.Name}");

            // 添加订单项
            var price = Money.Create(item.UnitPrice);
            order.AddItem(item.ProductId, item.ProductName, item.Quantity, price);

            // 减少库存
            product.DecreaseStock(item.Quantity, $"订单: {order.OrderNumber}");
        }

        // 保存订单
        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(order.Id);
    }
}
```

### 📚 完整示例总结

按照以上步骤，您可以添加任何新的聚合根：

1. **值对象** → `Domain/ValueObjects/`
2. **聚合根实体** → `Domain/Entities/`
3. **领域事件** → `Domain/Events/`
4. **仓储接口** → `Domain/Repositories/`
5. **仓储实现** → `Infrastructure/Persistence/Repositories/`
6. **Commands/Queries** → `Application/{AggregateRoot}/Commands|Queries/`
7. **事件处理器** → `Application/{AggregateRoot}/EventHandlers/`

---

## 2. 实现事件溯源

### 📍 在哪里实现

事件溯源需要在基础设施层添加事件存储：

```
src/
├── CommonMethodLibrary.Domain/
│   └── Common/
│       └── IDomainEventStore.cs          # 🆕 事件存储接口
├── CommonMethodLibrary.Infrastructure/
│   ├── EventSourcing/
│   │   ├── EventStore.cs                 # 🆕 事件存储实现
│   │   ├── StoredEvent.cs                # 🆕 存储的事件模型
│   │   └── EventStoreDbContext.cs        # 🆕 事件存储数据库上下文
│   └── Persistence/
│       └── UnitOfWork.cs                 # ✏️ 修改保存时发布事件
```

### 🔨 怎么做

#### Step 1: 创建事件存储接口

**文件位置**: `src/CommonMethodLibrary.Domain/Common/IDomainEventStore.cs`

```csharp
namespace CommonMethodLibrary.Domain.Common;

/// <summary>
/// 领域事件存储接口 - 用于事件溯源
/// </summary>
public interface IDomainEventStore
{
    /// <summary>
    /// 保存领域事件
    /// </summary>
    Task SaveEventAsync<TEvent>(TEvent domainEvent, string aggregateType, int aggregateId, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    /// <summary>
    /// 获取聚合的所有事件
    /// </summary>
    Task<IEnumerable<StoredEvent>> GetEventsAsync(string aggregateType, int aggregateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定时间范围的事件
    /// </summary>
    Task<IEnumerable<StoredEvent>> GetEventsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定类型的所有事件
    /// </summary>
    Task<IEnumerable<StoredEvent>> GetEventsByTypeAsync(string eventType, CancellationToken cancellationToken = default);
}
```

#### Step 2: 创建存储事件模型

**文件位置**: `src/CommonMethodLibrary.Infrastructure/EventSourcing/StoredEvent.cs`

```csharp
namespace CommonMethodLibrary.Infrastructure.EventSourcing;

/// <summary>
/// 存储的领域事件
/// </summary>
public class StoredEvent
{
    public long Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public int AggregateId { get; set; }
    public string EventData { get; set; } = string.Empty;  // JSON格式
    public DateTime OccurredAt { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? CorrelationId { get; set; }
    public int Version { get; set; }

    public StoredEvent()
    {
        OccurredAt = DateTime.UtcNow;
    }

    public static StoredEvent Create<TEvent>(
        TEvent domainEvent,
        string aggregateType,
        int aggregateId,
        int version,
        string? userId = null,
        string? userName = null,
        string? correlationId = null)
    {
        return new StoredEvent
        {
            EventType = typeof(TEvent).Name,
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            EventData = System.Text.Json.JsonSerializer.Serialize(domainEvent),
            Version = version,
            UserId = userId,
            UserName = userName,
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };
    }
}
```

#### Step 3: 创建事件存储数据库上下文

**文件位置**: `src/CommonMethodLibrary.Infrastructure/EventSourcing/EventStoreDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;

namespace CommonMethodLibrary.Infrastructure.EventSourcing;

/// <summary>
/// 事件存储数据库上下文 - 单独的事件溯源数据库
/// </summary>
public class EventStoreDbContext : DbContext
{
    public DbSet<StoredEvent> Events { get; set; } = null!;

    public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StoredEvent>(entity =>
        {
            entity.ToTable("DomainEvents");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.AggregateType)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.AggregateId)
                .IsRequired();

            entity.Property(e => e.EventData)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.OccurredAt)
                .IsRequired();

            entity.Property(e => e.UserId)
                .HasMaxLength(100);

            entity.Property(e => e.UserName)
                .HasMaxLength(200);

            entity.Property(e => e.CorrelationId)
                .HasMaxLength(100);

            // 索引
            entity.HasIndex(e => new { e.AggregateType, e.AggregateId })
                .HasDatabaseName("IX_Events_Aggregate");

            entity.HasIndex(e => e.EventType)
                .HasDatabaseName("IX_Events_EventType");

            entity.HasIndex(e => e.OccurredAt)
                .HasDatabaseName("IX_Events_OccurredAt");

            entity.HasIndex(e => e.CorrelationId)
                .HasDatabaseName("IX_Events_CorrelationId");
        });
    }
}
```

#### Step 4: 实现事件存储

**文件位置**: `src/CommonMethodLibrary.Infrastructure/EventSourcing/EventStore.cs`

```csharp
using CommonMethodLibrary.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CommonMethodLibrary.Infrastructure.EventSourcing;

/// <summary>
/// 事件存储实现
/// </summary>
public class EventStore : IDomainEventStore
{
    private readonly EventStoreDbContext _context;

    public EventStore(EventStoreDbContext context)
    {
        _context = context;
    }

    public async Task SaveEventAsync<TEvent>(
        TEvent domainEvent,
        string aggregateType,
        int aggregateId,
        CancellationToken cancellationToken = default) where TEvent : IDomainEvent
    {
        // 获取当前版本号
        var currentVersion = await _context.Events
            .Where(e => e.AggregateType == aggregateType && e.AggregateId == aggregateId)
            .MaxAsync(e => (int?)e.Version, cancellationToken) ?? 0;

        var storedEvent = StoredEvent.Create(
            domainEvent,
            aggregateType,
            aggregateId,
            currentVersion + 1
        );

        await _context.Events.AddAsync(storedEvent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredEvent>> GetEventsAsync(
        string aggregateType,
        int aggregateId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Where(e => e.AggregateType == aggregateType && e.AggregateId == aggregateId)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredEvent>> GetEventsAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Where(e => e.OccurredAt >= from && e.OccurredAt <= to)
            .OrderBy(e => e.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredEvent>> GetEventsByTypeAsync(
        string eventType,
        CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Where(e => e.EventType == eventType)
            .OrderBy(e => e.OccurredAt)
            .ToListAsync(cancellationToken);
    }
}
```

#### Step 5: 修改 UnitOfWork 自动保存事件

**文件位置**: `src/CommonMethodLibrary.Infrastructure/Persistence/UnitOfWork.cs` (修改现有文件)

```csharp
using CommonMethodLibrary.Domain.Common;
using CommonMethodLibrary.Domain.Repositories;
using CommonMethodLibrary.Infrastructure.EventSourcing;
using MediatR;

namespace CommonMethodLibrary.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IDomainEventStore _eventStore;
    private readonly IMediator _mediator;

    public UnitOfWork(
        ApplicationDbContext context,
        IDomainEventStore eventStore,
        IMediator mediator)
    {
        _context = context;
        _eventStore = eventStore;
        _mediator = mediator;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 收集所有领域事件
        var domainEvents = _context.ChangeTracker
            .Entries<Entity<int>>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        // 保存到事件存储（事件溯源）
        foreach (var domainEvent in domainEvents)
        {
            var entry = _context.ChangeTracker
                .Entries<Entity<int>>()
                .First(e => e.Entity.DomainEvents.Contains(domainEvent));

            var aggregateType = entry.Entity.GetType().Name;
            var aggregateId = entry.Entity.Id;

            await _eventStore.SaveEventAsync(domainEvent, aggregateType, aggregateId, cancellationToken);
        }

        // 保存业务数据
        var result = await _context.SaveChangesAsync(cancellationToken);

        // 发布领域事件（用于事件处理器）
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        // 清除领域事件
        _context.ChangeTracker
            .Entries<Entity<int>>()
            .ToList()
            .ForEach(entry => entry.Entity.ClearDomainEvents());

        return result;
    }
}
```

#### Step 6: 注册服务

**文件位置**: `src/CommonMethodLibrary.WebAPI/Program.cs` (在现有代码中添加)

```csharp
// 注册事件存储数据库
builder.Services.AddDbContext<EventStoreDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EventStoreConnection"),
        b => b.MigrationsAssembly("CommonMethodLibrary.Infrastructure")
    )
);

// 注册事件存储
builder.Services.AddScoped<IDomainEventStore, EventStore>();
```

#### Step 7: 创建审计查询

**文件位置**: `src/CommonMethodLibrary.Application/Auditing/Queries/GetAggregateHistoryQuery.cs`

```csharp
using CommonMethodLibrary.Application.Common;
using CommonMethodLibrary.Domain.Common;
using CommonMethodLibrary.Infrastructure.EventSourcing;
using MediatR;

namespace CommonMethodLibrary.Application.Auditing.Queries;

/// <summary>
/// 获取聚合历史查询 - 用于审计
/// </summary>
public record GetAggregateHistoryQuery(string AggregateType, int AggregateId) : IQuery<Result<AggregateHistoryDto>>;

public class AggregateHistoryDto
{
    public string AggregateType { get; set; } = string.Empty;
    public int AggregateId { get; set; }
    public List<EventDto> Events { get; set; } = new();
}

public class EventDto
{
    public long EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public int Version { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
}

public class GetAggregateHistoryQueryHandler : IRequestHandler<GetAggregateHistoryQuery, Result<AggregateHistoryDto>>
{
    private readonly IDomainEventStore _eventStore;

    public GetAggregateHistoryQueryHandler(IDomainEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<Result<AggregateHistoryDto>> Handle(GetAggregateHistoryQuery request, CancellationToken cancellationToken)
    {
        var events = await _eventStore.GetEventsAsync(request.AggregateType, request.AggregateId, cancellationToken);

        var dto = new AggregateHistoryDto
        {
            AggregateType = request.AggregateType,
            AggregateId = request.AggregateId,
            Events = events.Select(e => new EventDto
            {
                EventId = e.Id,
                EventType = e.EventType,
                EventData = e.EventData,
                OccurredAt = e.OccurredAt,
                Version = e.Version,
                UserId = e.UserId,
                UserName = e.UserName
            }).ToList()
        };

        return Result<AggregateHistoryDto>.Success(dto);
    }
}
```

### 📚 事件溯源的优势

1. **完整审计日志** - 保存所有变更历史
2. **时间旅行** - 可以重放事件恢复任意时间点的状态
3. **数据分析** - 分析业务趋势和用户行为
4. **调试和故障排查** - 完整追踪问题发生过程

---

## 3. 添加消息队列

### 📍 在哪里集成

消息队列用于处理分布式事件和异步任务：

```
src/
├── CommonMethodLibrary.Infrastructure/
│   ├── MessageQueue/
│   │   ├── IMessagePublisher.cs         # 🆕 消息发布接口
│   │   ├── IMessageConsumer.cs          # 🆕 消息消费接口
│   │   ├── RabbitMQ/
│   │   │   ├── RabbitMQPublisher.cs     # 🆕 RabbitMQ发布实现
│   │   │   ├── RabbitMQConsumer.cs      # 🆕 RabbitMQ消费实现
│   │   │   └── RabbitMQConnection.cs    # 🆕 RabbitMQ连接管理
│   │   └── Kafka/
│   │       ├── KafkaPublisher.cs        # 🆕 Kafka发布实现
│   │       └── KafkaConsumer.cs         # 🆕 Kafka消费实现
│   └── BackgroundJobs/
│       └── MessageConsumerHostedService.cs  # 🆕 消息消费后台服务
```

### 🔨 怎么做

#### Step 1: 安装 NuGet 包

```bash
# RabbitMQ
dotnet add package RabbitMQ.Client

# Kafka (可选)
dotnet add package Confluent.Kafka
```

#### Step 2: 创建消息接口

**文件位置**: `src/CommonMethodLibrary.Infrastructure/MessageQueue/IMessagePublisher.cs`

```csharp
namespace CommonMethodLibrary.Infrastructure.MessageQueue;

/// <summary>
/// 消息发布接口
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// 发布消息
    /// </summary>
    Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发布领域事件
    /// </summary>
    Task PublishEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default);
}
```

**文件位置**: `src/CommonMethodLibrary.Infrastructure/MessageQueue/IMessageConsumer.cs`

```csharp
namespace CommonMethodLibrary.Infrastructure.MessageQueue;

/// <summary>
/// 消息消费接口
/// </summary>
public interface IMessageConsumer
{
    /// <summary>
    /// 订阅消息
    /// </summary>
    void Subscribe<T>(string queueName, Func<T, Task> handler);

    /// <summary>
    /// 开始消费
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止消费
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}
```

#### Step 3: 实现 RabbitMQ 发布者

**文件位置**: `src/CommonMethodLibrary.Infrastructure/MessageQueue/RabbitMQ/RabbitMQConnection.cs`

```csharp
using RabbitMQ.Client;

namespace CommonMethodLibrary.Infrastructure.MessageQueue.RabbitMQ;

/// <summary>
/// RabbitMQ 连接管理
/// </summary>
public class RabbitMQConnection : IDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();

    public RabbitMQConnection(string hostName, int port = 5672, string userName = "guest", string password = "guest")
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
    }

    public IModel GetChannel()
    {
        if (_channel != null && _channel.IsOpen)
            return _channel;

        lock (_lock)
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _connection = _connectionFactory.CreateConnection();
            }

            _channel = _connection.CreateModel();
            return _channel;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
```

**文件位置**: `src/CommonMethodLibrary.Infrastructure/MessageQueue/RabbitMQ/RabbitMQPublisher.cs`

```csharp
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace CommonMethodLibrary.Infrastructure.MessageQueue.RabbitMQ;

/// <summary>
/// RabbitMQ 消息发布实现
/// </summary>
public class RabbitMQPublisher : IMessagePublisher, IDisposable
{
    private readonly RabbitMQConnection _connection;
    private readonly IModel _channel;

    public RabbitMQPublisher(RabbitMQConnection connection)
    {
        _connection = connection;
        _channel = _connection.GetChannel();

        // 声明默认的事件交换机
        _channel.ExchangeDeclare(
            exchange: "domain_events",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );
    }

    public Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Type = typeof(T).Name;
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        _channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: body
        );

        return Task.CompletedTask;
    }

    public Task PublishEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = typeof(T).Name;
        var routingKey = $"event.{eventType}";

        return PublishAsync("domain_events", routingKey, domainEvent, cancellationToken);
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
    }
}
```

#### Step 4: 实现 RabbitMQ 消费者

**文件位置**: `src/CommonMethodLibrary.Infrastructure/MessageQueue/RabbitMQ/RabbitMQConsumer.cs`

```csharp
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommonMethodLibrary.Infrastructure.MessageQueue.RabbitMQ;

/// <summary>
/// RabbitMQ 消息消费实现
/// </summary>
public class RabbitMQConsumer : IMessageConsumer, IDisposable
{
    private readonly RabbitMQConnection _connection;
    private readonly IModel _channel;
    private readonly Dictionary<string, Delegate> _handlers = new();

    public RabbitMQConsumer(RabbitMQConnection connection)
    {
        _connection = connection;
        _channel = _connection.GetChannel();
    }

    public void Subscribe<T>(string queueName, Func<T, Task> handler)
    {
        // 声明队列
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        // 绑定到事件交换机
        var eventType = typeof(T).Name;
        _channel.QueueBind(
            queue: queueName,
            exchange: "domain_events",
            routingKey: $"event.{eventType}"
        );

        _handlers[queueName] = handler;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (queueName, handler) in _handlers)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    // 获取消息类型
                    var messageType = handler.GetType().GetGenericArguments()[0];
                    var message = JsonSerializer.Deserialize(json, messageType);

                    if (message != null)
                    {
                        await ((dynamic)handler).Invoke((dynamic)message);
                    }

                    // 确认消息
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    // 拒绝消息并重新入队
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                    Console.WriteLine($"消息处理失败: {ex.Message}");
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
    }
}
```

#### Step 5: 创建消费者后台服务

**文件位置**: `src/CommonMethodLibrary.Infrastructure/BackgroundJobs/MessageConsumerHostedService.cs`

```csharp
using CommonMethodLibrary.Domain.Events;
using CommonMethodLibrary.Infrastructure.MessageQueue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommonMethodLibrary.Infrastructure.BackgroundJobs;

/// <summary>
/// 消息消费后台服务
/// </summary>
public class MessageConsumerHostedService : IHostedService
{
    private readonly IMessageConsumer _consumer;
    private readonly ILogger<MessageConsumerHostedService> _logger;

    public MessageConsumerHostedService(
        IMessageConsumer consumer,
        ILogger<MessageConsumerHostedService> logger)
    {
        _consumer = consumer;
        _logger = logger;

        // 订阅事件
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        // 订阅用户创建事件
        _consumer.Subscribe<UserCreatedEvent>("user_created_queue", async (evt) =>
        {
            _logger.LogInformation($"处理用户创建事件: {evt.Username}");
            // 这里可以做一些异步操作，如发送欢迎邮件
            await Task.Delay(100);
        });

        // 订阅订单支付事件
        _consumer.Subscribe<OrderPaidEvent>("order_paid_queue", async (evt) =>
        {
            _logger.LogInformation($"处理订单支付事件: {evt.OrderNumber}");
            // 这里可以触发发货流程
            await Task.Delay(100);
        });

        // 可以继续订阅更多事件...
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("消息消费服务启动");
        await _consumer.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("消息消费服务停止");
        await _consumer.StopAsync(cancellationToken);
    }
}
```

#### Step 6: 修改 UnitOfWork 发布到消息队列

**文件位置**: `src/CommonMethodLibrary.Infrastructure/Persistence/UnitOfWork.cs` (继续修改)

```csharp
using CommonMethodLibrary.Infrastructure.MessageQueue;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IDomainEventStore _eventStore;
    private readonly IMediator _mediator;
    private readonly IMessagePublisher _messagePublisher;  // 🆕 添加

    public UnitOfWork(
        ApplicationDbContext context,
        IDomainEventStore eventStore,
        IMediator mediator,
        IMessagePublisher messagePublisher)  // 🆕 添加
    {
        _context = context;
        _eventStore = eventStore;
        _mediator = mediator;
        _messagePublisher = messagePublisher;  // 🆕 添加
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // ... 前面的代码不变 ...

        // 保存业务数据
        var result = await _context.SaveChangesAsync(cancellationToken);

        // 发布领域事件到本地处理器
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        // 🆕 发布领域事件到消息队列（用于分布式处理）
        foreach (var domainEvent in domainEvents)
        {
            await _messagePublisher.PublishEventAsync(domainEvent, cancellationToken);
        }

        // ... 后面的代码不变 ...
    }
}
```

#### Step 7: 注册服务

**文件位置**: `src/CommonMethodLibrary.WebAPI/Program.cs` (添加)

```csharp
// 配置 RabbitMQ
var rabbitMQHost = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
var rabbitMQConnection = new RabbitMQConnection(rabbitMQHost);

builder.Services.AddSingleton(rabbitMQConnection);
builder.Services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
builder.Services.AddSingleton<IMessageConsumer, RabbitMQConsumer>();

// 注册消息消费后台服务
builder.Services.AddHostedService<MessageConsumerHostedService>();
```

**文件位置**: `src/CommonMethodLibrary.WebAPI/appsettings.json` (添加配置)

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
```

### 📚 消息队列的优势

1. **异步处理** - 提高系统响应速度
2. **解耦服务** - 服务之间松耦合
3. **削峰填谷** - 应对突发流量
4. **可靠性** - 消息持久化和重试机制

---

## 4. 实现读写分离

### 📍 在哪里实现

读写分离（CQRS的延伸）使用单独的读库：

```
src/
├── CommonMethodLibrary.Application/
│   └── Common/
│       └── IQueryDbContext.cs           # 🆕 查询数据库上下文接口
├── CommonMethodLibrary.Infrastructure/
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs      # ✅ 写库（已存在）
│   │   ├── QueryDbContext.cs            # 🆕 读库
│   │   └── ReadModels/                  # 🆕 读模型（扁平化、非规范化）
│   │       ├── UserReadModel.cs
│   │       ├── OrderReadModel.cs
│   │       └── ProductReadModel.cs
│   └── Synchronization/
│       └── ReadModelSynchronizer.cs     # 🆕 读模型同步器
```

### 🔨 怎么做

#### Step 1: 创建读模型

读模型是扁平化的、为查询优化的数据结构：

**文件位置**: `src/CommonMethodLibrary.Infrastructure/Persistence/ReadModels/UserReadModel.cs`

```csharp
namespace CommonMethodLibrary.Infrastructure.Persistence.ReadModels;

/// <summary>
/// 用户读模型 - 为查询优化，扁平化结构
/// </summary>
public class UserReadModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? RealName { get; set; }
    public int? Age { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // 扁平化的地址信息（非规范化）
    public string? AddressProvince { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressDistrict { get; set; }
    public string? AddressStreet { get; set; }
    public string? AddressPostalCode { get; set; }
    public string? FullAddress { get; set; }  // 预计算的完整地址

    // 统计信息（非规范化）
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }
}
```

**文件位置**: `src/CommonMethodLibrary.Infrastructure/Persistence/ReadModels/OrderReadModel.cs`

```csharp
namespace CommonMethodLibrary.Infrastructure.Persistence.ReadModels;

/// <summary>
/// 订单读模型 - 包含所有需要展示的信息，避免JOIN
/// </summary>
public class OrderReadModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }

    // 冗余用户信息（避免JOIN）
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    // 订单信息
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "CNY";
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }  // 商品总数

    // 地址信息（扁平化）
    public string ShippingProvince { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingDistrict { get; set; } = string.Empty;
    public string ShippingStreet { get; set; } = string.Empty;
    public string? ShippingPostalCode { get; set; }
    public string FullShippingAddress { get; set; } = string.Empty;

    // 时间信息
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public string? Remark { get; set; }

    // 订单明细（JSON存储，避免多表JOIN）
    public string ItemsJson { get; set; } = "[]";
}
```

#### Step 2: 创建查询数据库上下文

**文件位置**: `src/CommonMethodLibrary.Infrastructure/Persistence/QueryDbContext.cs`

```csharp
using CommonMethodLibrary.Infrastructure.Persistence.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace CommonMethodLibrary.Infrastructure.Persistence;

/// <summary>
/// 查询数据库上下文 - 只读数据库
/// </summary>
public class QueryDbContext : DbContext
{
    public DbSet<UserReadModel> Users { get; set; } = null!;
    public DbSet<OrderReadModel> Orders { get; set; } = null!;
    public DbSet<ProductReadModel> Products { get; set; } = null!;

    public QueryDbContext(DbContextOptions<QueryDbContext> options)
        : base(options)
    {
        // 配置为只读
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 用户读模型配置
        modelBuilder.Entity<UserReadModel>(entity =>
        {
            entity.ToTable("Users_ReadModel");
            entity.HasKey(e => e.Id);

            // 添加查询优化的索引
            entity.HasIndex(e => e.Username);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.TotalOrders);
        });

        // 订单读模型配置
        modelBuilder.Entity<OrderReadModel>(entity =>
        {
            entity.ToTable("Orders_ReadModel");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });

        // 产品读模型配置
        modelBuilder.Entity<ProductReadModel>(entity =>
        {
            entity.ToTable("Products_ReadModel");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.StockQuantity);
        });
    }
}
```

#### Step 3: 创建读模型同步器

**文件位置**: `src/CommonMethodLibrary.Infrastructure/Synchronization/ReadModelSynchronizer.cs`

```csharp
using CommonMethodLibrary.Domain.Events;
using CommonMethodLibrary.Infrastructure.Persistence;
using CommonMethodLibrary.Infrastructure.Persistence.ReadModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommonMethodLibrary.Infrastructure.Synchronization;

/// <summary>
/// 读模型同步器 - 监听领域事件并更新读模型
/// </summary>
public class UserCreatedEventToReadModelHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly QueryDbContext _queryContext;

    public UserCreatedEventToReadModelHandler(QueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        // 从写库读取完整数据
        var user = await _queryContext.Users.FindAsync(new object[] { notification.UserId }, cancellationToken);

        if (user == null)
        {
            // 创建新的读模型
            var readModel = new UserReadModel
            {
                Id = notification.UserId,
                Username = notification.Username,
                Email = notification.Email,
                CreatedAt = DateTime.UtcNow,
                Status = "Active",
                IsActive = true,
                TotalOrders = 0,
                TotalSpent = 0
            };

            await _queryContext.Users.AddAsync(readModel, cancellationToken);
            await _queryContext.SaveChangesAsync(cancellationToken);
        }
    }
}

/// <summary>
/// 订单创建事件处理器 - 更新用户统计信息
/// </summary>
public class OrderPaidEventToReadModelHandler : INotificationHandler<OrderPaidEvent>
{
    private readonly QueryDbContext _queryContext;

    public OrderPaidEventToReadModelHandler(QueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        // 更新用户读模型的统计信息
        var userReadModel = await _queryContext.Users.FindAsync(new object[] { notification.UserId }, cancellationToken);
        if (userReadModel != null)
        {
            userReadModel.TotalOrders++;
            userReadModel.TotalSpent += notification.TotalAmount.Amount;
            userReadModel.LastOrderDate = DateTime.UtcNow;

            await _queryContext.SaveChangesAsync(cancellationToken);
        }

        // 创建订单读模型
        // ... (类似的逻辑)
    }
}
```

#### Step 4: 修改查询使用读库

**文件位置**: `src/CommonMethodLibrary.Application/Users/Queries/GetPagedUsersQuery.cs` (修改现有)

```csharp
using CommonMethodLibrary.Infrastructure.Persistence;  // 🆕 使用QueryDbContext
using Microsoft.EntityFrameworkCore;

public class GetPagedUsersQueryHandler : IRequestHandler<GetPagedUsersQuery, Result<PagedResult<UserDto>>>
{
    private readonly QueryDbContext _queryContext;  // 🆕 改用读库

    public GetPagedUsersQueryHandler(QueryDbContext queryContext)
    {
        _queryContext = queryContext;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetPagedUsersQuery request, CancellationToken cancellationToken)
    {
        // 使用读模型查询，性能更好
        var query = _queryContext.Users.AsQueryable();

        // 应用过滤
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(u =>
                u.Username.Contains(request.SearchTerm) ||
                u.Email.Contains(request.SearchTerm) ||
                (u.RealName != null && u.RealName.Contains(request.SearchTerm))
            );
        }

        // 统计总数
        var totalCount = await query.CountAsync(cancellationToken);

        // 分页和排序
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // 映射到DTO
        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            RealName = u.RealName,
            Age = u.Age,
            IsActive = u.IsActive,
            Status = u.Status,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt,
            LastLoginAt = u.LastLoginAt,
            // 🆕 读模型的额外信息
            FullAddress = u.FullAddress,
            TotalOrders = u.TotalOrders,
            TotalSpent = u.TotalSpent
        }).ToList();

        var result = new PagedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<UserDto>>.Success(result);
    }
}
```

#### Step 5: 注册服务

**文件位置**: `src/CommonMethodLibrary.WebAPI/Program.cs` (添加)

```csharp
// 写库（主库）
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("CommonMethodLibrary.Infrastructure")
    )
);

// 🆕 读库（从库或只读副本）
builder.Services.AddDbContext<QueryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("QueryConnection"),  // 不同的连接字符串
        b => b.MigrationsAssembly("CommonMethodLibrary.Infrastructure")
    )
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)  // 只读优化
);
```

**文件位置**: `src/CommonMethodLibrary.WebAPI/appsettings.json` (添加配置)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=主库服务器;Database=AppDb;...",
    "QueryConnection": "Server=从库服务器;Database=AppDb_ReadOnly;ApplicationIntent=ReadOnly;..."
  }
}
```

### 📚 读写分离的优势

1. **性能提升** - 查询不影响写入性能
2. **扩展性** - 可以有多个只读副本
3. **优化查询** - 读模型可以针对查询场景优化
4. **降低主库压力** - 大量查询分流到从库

---

## 5. 添加微服务

### 📍 如何拆分

将单体应用拆分为多个微服务：

```
解决方案结构:
├── Services/
│   ├── UserService/                     # 🆕 用户服务
│   │   ├── UserService.API/
│   │   ├── UserService.Domain/
│   │   ├── UserService.Application/
│   │   └── UserService.Infrastructure/
│   │
│   ├── OrderService/                    # 🆕 订单服务
│   │   ├── OrderService.API/
│   │   ├── OrderService.Domain/
│   │   ├── OrderService.Application/
│   │   └── OrderService.Infrastructure/
│   │
│   ├── ProductService/                  # 🆕 产品服务
│   │   ├── ProductService.API/
│   │   ├── ProductService.Domain/
│   │   ├── ProductService.Application/
│   │   └── ProductService.Infrastructure/
│   │
│   └── Shared/                          # 🆕 共享库
│       ├── Shared.Domain/               # 共享领域模型
│       ├── Shared.Infrastructure/       # 共享基础设施
│       └── Shared.Contracts/            # 服务间通信契约
│
├── ApiGateway/                          # 🆕 API网关（Ocelot/YARP）
└── docker-compose.yml                   # 🆕 Docker编排
```

### 🔨 怎么做

#### Step 1: 识别边界上下文

根据 DDD 的限界上下文（Bounded Context）拆分：

1. **用户上下文** - 用户管理、认证授权
2. **订单上下文** - 订单创建、支付、发货
3. **产品上下文** - 产品管理、库存管理
4. **支付上下文** - 支付处理（可选）
5. **通知上下文** - 邮件、短信通知（可选）

#### Step 2: 创建共享契约

**文件位置**: `Services/Shared/Shared.Contracts/Events/IntegrationEvents.cs`

```csharp
namespace Shared.Contracts.Events;

/// <summary>
/// 集成事件 - 用于服务间通信
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

/// <summary>
/// 用户已创建集成事件
/// </summary>
public record UserCreatedIntegrationEvent(
    int UserId,
    string Username,
    string Email,
    Guid EventId,
    DateTime OccurredAt
) : IIntegrationEvent;

/// <summary>
/// 订单已支付集成事件
/// </summary>
public record OrderPaidIntegrationEvent(
    int OrderId,
    string OrderNumber,
    int UserId,
    decimal TotalAmount,
    string Currency,
    Guid EventId,
    DateTime OccurredAt
) : IIntegrationEvent;

/// <summary>
/// 库存已预留集成事件
/// </summary>
public record StockReservedIntegrationEvent(
    int ProductId,
    int Quantity,
    string OrderNumber,
    Guid EventId,
    DateTime OccurredAt
) : IIntegrationEvent;
```

#### Step 3: 创建独立的订单服务

**文件位置**: `Services/OrderService/OrderService.API/Program.cs`

```csharp
using OrderService.Infrastructure;
using OrderService.Application;
using Shared.Infrastructure.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

// 添加服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加数据库
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDatabase"))
);

// 添加MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly)
);

// 添加消息队列
var rabbitMQConnection = new RabbitMQConnection(
    builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost"
);
builder.Services.AddSingleton(rabbitMQConnection);
builder.Services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
builder.Services.AddSingleton<IMessageConsumer, RabbitMQConsumer>();

// 添加仓储
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### Step 4: 服务间通信 - 使用事件总线

**文件位置**: `Services/OrderService/OrderService.Application/IntegrationEvents/OrderPaidIntegrationEventPublisher.cs`

```csharp
using Shared.Contracts.Events;
using Shared.Infrastructure.MessageQueue;
using MediatR;
using OrderService.Domain.Events;

namespace OrderService.Application.IntegrationEvents;

/// <summary>
/// 将领域事件转换为集成事件并发布
/// </summary>
public class OrderPaidDomainEventHandler : INotificationHandler<OrderPaidEvent>
{
    private readonly IMessagePublisher _messagePublisher;

    public OrderPaidDomainEventHandler(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        // 转换为集成事件
        var integrationEvent = new OrderPaidIntegrationEvent(
            OrderId: notification.OrderId,
            OrderNumber: notification.OrderNumber,
            UserId: notification.UserId,
            TotalAmount: notification.TotalAmount.Amount,
            Currency: notification.TotalAmount.Currency,
            EventId: Guid.NewGuid(),
            OccurredAt: DateTime.UtcNow
        );

        // 发布到消息队列，其他服务可以订阅
        await _messagePublisher.PublishAsync(
            exchange: "integration_events",
            routingKey: "order.paid",
            message: integrationEvent,
            cancellationToken
        );
    }
}
```

**文件位置**: `Services/UserService/UserService.Application/IntegrationEvents/OrderPaidIntegrationEventConsumer.cs`

```csharp
using Shared.Contracts.Events;
using Shared.Infrastructure.MessageQueue;

namespace UserService.Application.IntegrationEvents;

/// <summary>
/// 用户服务订阅订单支付事件 - 更新用户统计
/// </summary>
public class OrderPaidIntegrationEventHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderPaidIntegrationEventHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(OrderPaidIntegrationEvent evt)
    {
        // 更新用户的订单统计
        var user = await _userRepository.GetByIdAsync(evt.UserId);
        if (user != null)
        {
            // 这里可以更新用户的统计信息
            // user.UpdateOrderStatistics(evt.TotalAmount);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}

/// <summary>
/// 注册消费者的后台服务
/// </summary>
public class IntegrationEventConsumerHostedService : IHostedService
{
    private readonly IMessageConsumer _consumer;
    private readonly IServiceProvider _serviceProvider;

    public IntegrationEventConsumerHostedService(
        IMessageConsumer consumer,
        IServiceProvider serviceProvider)
    {
        _consumer = consumer;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // 订阅订单支付事件
        _consumer.Subscribe<OrderPaidIntegrationEvent>(
            "user_service_order_paid_queue",
            async (evt) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<OrderPaidIntegrationEventHandler>();
                await handler.HandleAsync(evt);
            }
        );

        await _consumer.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _consumer.StopAsync(cancellationToken);
    }
}
```

#### Step 5: API 网关配置

安装 Ocelot 或 YARP（Yet Another Reverse Proxy）

**文件位置**: `ApiGateway/ocelot.json`

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/users/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "user-service",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/api/users/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
    },
    {
      "DownstreamPathTemplate": "/api/orders/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "order-service",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/api/orders/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
    },
    {
      "DownstreamPathTemplate": "/api/products/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "product-service",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/api/products/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://api-gateway"
  }
}
```

#### Step 6: Docker Compose 编排

**文件位置**: `docker-compose.yml`

```yaml
version: '3.8'

services:
  # 消息队列
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: admin123

  # 用户服务数据库
  user-db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong@Passw0rd"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"

  # 订单服务数据库
  order-db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong@Passw0rd"
      ACCEPT_EULA: "Y"
    ports:
      - "1434:1433"

  # 产品服务数据库
  product-db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong@Passw0rd"
      ACCEPT_EULA: "Y"
    ports:
      - "1435:1433"

  # 用户服务
  user-service:
    build:
      context: ./Services/UserService
      dockerfile: Dockerfile
    environment:
      ConnectionStrings__DefaultConnection: "Server=user-db;Database=UserDb;..."
      RabbitMQ__Host: rabbitmq
    depends_on:
      - user-db
      - rabbitmq
    ports:
      - "5001:80"

  # 订单服务
  order-service:
    build:
      context: ./Services/OrderService
      dockerfile: Dockerfile
    environment:
      ConnectionStrings__DefaultConnection: "Server=order-db;Database=OrderDb;..."
      RabbitMQ__Host: rabbitmq
    depends_on:
      - order-db
      - rabbitmq
    ports:
      - "5002:80"

  # 产品服务
  product-service:
    build:
      context: ./Services/ProductService
      dockerfile: Dockerfile
    environment:
      ConnectionStrings__DefaultConnection: "Server=product-db;Database=ProductDb;..."
      RabbitMQ__Host: rabbitmq
    depends_on:
      - product-db
      - rabbitmq
    ports:
      - "5003:80"

  # API 网关
  api-gateway:
    build:
      context: ./ApiGateway
      dockerfile: Dockerfile
    ports:
      - "5000:80"
    depends_on:
      - user-service
      - order-service
      - product-service
```

#### Step 7: 启动微服务

```bash
# 启动所有服务
docker-compose up -d

# 查看服务状态
docker-compose ps

# 查看日志
docker-compose logs -f order-service
```

### 📚 微服务架构的优势

1. **独立部署** - 每个服务可以独立发布
2. **技术栈灵活** - 不同服务可以使用不同技术
3. **团队自治** - 不同团队负责不同服务
4. **故障隔离** - 一个服务故障不影响其他服务
5. **水平扩展** - 可以针对性地扩展高负载服务

---

## 🎯 总结

### 扩展路径建议

1. **第一阶段**: 扩展聚合根 → 添加更多业务实体
2. **第二阶段**: 实现事件溯源 → 完整的审计日志
3. **第三阶段**: 添加消息队列 → 异步处理和解耦
4. **第四阶段**: 实现读写分离 → 提升查询性能
5. **第五阶段**: 拆分微服务 → 完整的分布式架构

### 最佳实践

1. **逐步演进** - 不要一次性实现所有功能
2. **测试驱动** - 每个扩展都要有完整的测试
3. **文档先行** - 先设计API契约和数据模型
4. **监控告警** - 添加日志、指标和分布式追踪
5. **持续集成** - 使用CI/CD自动化部署

### 相关资源

- **DDD**: Eric Evans 的《领域驱动设计》
- **CQRS**: Greg Young 的 CQRS 文档
- **Event Sourcing**: Martin Fowler 的事件溯源文章
- **微服务**: Chris Richardson 的《微服务架构设计模式》
- **消息队列**: RabbitMQ 和 Kafka 官方文档

---

**注意**: 本文档基于现有的 DDD + CQRS 架构，所有示例代码都可以直接应用到项目中。建议先在开发环境验证，然后再部署到生产环境。
