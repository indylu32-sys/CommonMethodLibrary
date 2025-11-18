using CommonMethodLibrary.Domain.Entities;
using CommonMethodLibrary.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonMethodLibrary.Infrastructure.Persistence.Configurations;

/// <summary>
/// 用户实体配置 - EF Core Fluent API配置
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.Username)
            .IsUnique();

        // 配置值对象 - Email
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value)
            );

        builder.HasIndex(u => u.Email)
            .IsUnique();

        // 配置值对象 - PhoneNumber
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20)
            .HasConversion(
                phone => phone != null ? phone.Value : null,
                value => value != null ? PhoneNumber.Create(value) : null
            );

        // 配置值对象 - Address
        builder.OwnsOne(u => u.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Province).HasMaxLength(50);
            addressBuilder.Property(a => a.City).HasMaxLength(50);
            addressBuilder.Property(a => a.District).HasMaxLength(50);
            addressBuilder.Property(a => a.Street).HasMaxLength(200);
            addressBuilder.Property(a => a.PostalCode).HasMaxLength(10);
        });

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.RealName)
            .HasMaxLength(50);

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.Version)
            .IsConcurrencyToken();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Ignore(u => u.DomainEvents);

        // 种子数据
        builder.HasData(
            CreateUser(1, "admin", "admin@example.com"),
            CreateUser(2, "testuser", "test@example.com")
        );
    }

    private static User CreateUser(int id, string username, string email)
    {
        var user = User.Create(
            username,
            email,
            "AQAAAAEAACcQAAAAEDummyHashForDemo123456789",
            "13800138000",
            "示例用户",
            30
        );

        // 使用反射设置ID（仅用于种子数据）
        var idProperty = typeof(User).BaseType!.GetProperty("Id");
        idProperty!.SetValue(user, id);

        user.ClearDomainEvents(); // 清除种子数据的事件

        return user;
    }
}
