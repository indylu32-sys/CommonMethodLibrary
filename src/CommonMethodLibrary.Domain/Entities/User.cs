using CommonMethodLibrary.Domain.Common;
using CommonMethodLibrary.Domain.Events;
using CommonMethodLibrary.Domain.ValueObjects;

namespace CommonMethodLibrary.Domain.Entities;

/// <summary>
/// 用户聚合根 - 用户领域的核心实体
/// </summary>
public class User : AggregateRoot<int>
{
    public string Username { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; }
    public string? RealName { get; private set; }
    public int? Age { get; private set; }
    public Address? Address { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public UserStatus Status { get; private set; }

    // EF Core 需要的无参构造函数
    private User()
    {
        Username = string.Empty;
        Email = null!;
        PasswordHash = string.Empty;
    }

    private User(
        string username,
        Email email,
        string passwordHash,
        PhoneNumber? phoneNumber = null,
        string? realName = null,
        int? age = null)
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        PhoneNumber = phoneNumber;
        RealName = realName;
        Age = age;
        IsActive = true;
        Status = UserStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 创建新用户 - 工厂方法
    /// </summary>
    public static User Create(
        string username,
        string email,
        string passwordHash,
        string? phoneNumber = null,
        string? realName = null,
        int? age = null)
    {
        // 验证用户名
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("用户名不能为空", nameof(username));

        if (username.Length < 3 || username.Length > 50)
            throw new ArgumentException("用户名长度必须在3-50个字符之间", nameof(username));

        // 验证年龄
        if (age.HasValue && (age.Value < 1 || age.Value > 150))
            throw new ArgumentException("年龄必须在1-150之间", nameof(age));

        // 创建值对象
        var emailVO = Email.Create(email);
        var phoneNumberVO = !string.IsNullOrWhiteSpace(phoneNumber)
            ? PhoneNumber.Create(phoneNumber)
            : null;

        var user = new User(username, emailVO, passwordHash, phoneNumberVO, realName, age);

        // 发布用户创建事件
        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Username, user.Email.Value));

        return user;
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    public void UpdateProfile(string? username, string? email, string? phoneNumber, string? realName, int? age)
    {
        var hasChanges = false;

        if (!string.IsNullOrWhiteSpace(username) && username != Username)
        {
            if (username.Length < 3 || username.Length > 50)
                throw new ArgumentException("用户名长度必须在3-50个字符之间", nameof(username));

            Username = username;
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailVO = Email.Create(email);
            if (emailVO != Email)
            {
                Email = emailVO;
                hasChanges = true;
            }
        }

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var phoneNumberVO = PhoneNumber.Create(phoneNumber);
            if (phoneNumberVO != PhoneNumber)
            {
                PhoneNumber = phoneNumberVO;
                hasChanges = true;
            }
        }

        if (realName != null && realName != RealName)
        {
            RealName = realName;
            hasChanges = true;
        }

        if (age.HasValue && age != Age)
        {
            if (age.Value < 1 || age.Value > 150)
                throw new ArgumentException("年龄必须在1-150之间", nameof(age));

            Age = age;
            hasChanges = true;
        }

        if (hasChanges)
        {
            MarkAsUpdated();
            IncrementVersion();
            AddDomainEvent(new UserProfileUpdatedEvent(Id, Username));
        }
    }

    /// <summary>
    /// 设置地址
    /// </summary>
    public void SetAddress(string province, string city, string district, string street, string? postalCode = null)
    {
        Address = Address.Create(province, city, district, street, postalCode);
        MarkAsUpdated();
        IncrementVersion();
    }

    /// <summary>
    /// 激活用户
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        Status = UserStatus.Active;
        MarkAsUpdated();
        IncrementVersion();

        AddDomainEvent(new UserActivatedEvent(Id, Username));
    }

    /// <summary>
    /// 禁用用户
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        Status = UserStatus.Inactive;
        MarkAsUpdated();
        IncrementVersion();

        AddDomainEvent(new UserDeactivatedEvent(Id, Username));
    }

    /// <summary>
    /// 记录登录
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MarkAsUpdated();

        AddDomainEvent(new UserLoggedInEvent(Id, Username, LastLoginAt.Value));
    }

    /// <summary>
    /// 更改密码
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("密码哈希不能为空", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        MarkAsUpdated();
        IncrementVersion();

        AddDomainEvent(new UserPasswordChangedEvent(Id, Username));
    }
}

/// <summary>
/// 用户状态枚举
/// </summary>
public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Deleted = 4
}
