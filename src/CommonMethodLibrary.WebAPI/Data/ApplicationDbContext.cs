using CommonMethodLibrary.WebAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommonMethodLibrary.WebAPI.Data;

/// <summary>
/// 应用程序数据库上下文
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置User实体
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
        });

        // 种子数据
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@example.com",
                PhoneNumber = "13800138000",
                PasswordHash = "AQAAAAEAACcQAAAAEDummyHashForDemo123456789", // 实际应用中应该使用真实的密码哈希
                RealName = "管理员",
                Age = 30,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 2,
                Username = "testuser",
                Email = "test@example.com",
                PhoneNumber = "13900139000",
                PasswordHash = "AQAAAAEAACcQAAAAEDummyHashForDemo987654321",
                RealName = "测试用户",
                Age = 25,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
