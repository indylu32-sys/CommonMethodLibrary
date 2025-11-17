using CommonMethodLibrary.Core.Helpers;
using CommonMethodLibrary.WebAPI.Data;
using CommonMethodLibrary.WebAPI.Models.DTOs;
using CommonMethodLibrary.WebAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CommonMethodLibrary.Tests.Services;

/// <summary>
/// UserService单元测试
/// 演示如何测试使用了工具库的服务
/// </summary>
public class UserServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly CacheHelper _cache;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // 创建内存数据库
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<UserService>>();
        _cache = new CacheHelper();

        _userService = new UserService(_context, _loggerMock.Object, _cache);

        // 初始化测试数据
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "newuser",
            Email = "newuser@example.com",
            PhoneNumber = "13900139000",
            Password = "Password123!",
            RealName = "New User",
            Age = 25
        };

        // Act
        var user = await _userService.CreateUserAsync(dto);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(dto.Username, user.Username);
        Assert.Equal(dto.Email, user.Email);
        Assert.NotEmpty(user.PasswordHash);

        // 验证密码哈希是使用SHA256生成的
        var expectedHash = EncryptionHelper.Sha256(dto.Password);
        Assert.Equal(expectedHash, user.PasswordHash);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "testuser1",
            Email = "testuser1@example.com",
            Password = "Password123!"
        };
        var createdUser = await _userService.CreateUserAsync(dto);

        // Act
        var user = await _userService.GetUserByIdAsync(createdUser.Id);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(createdUser.Id, user.Id);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateUserFields()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            Username = "updatetest",
            Email = "updatetest@example.com",
            Password = "Password123!"
        };
        var createdUser = await _userService.CreateUserAsync(createDto);

        var updateDto = new UpdateUserDto
        {
            RealName = "Updated Name",
            Age = 30
        };

        // Act
        var updatedUser = await _userService.UpdateUserAsync(createdUser.Id, updateDto);

        // Assert
        Assert.NotNull(updatedUser);
        Assert.Equal("Updated Name", updatedUser.RealName);
        Assert.Equal(30, updatedUser.Age);
        Assert.NotNull(updatedUser.UpdatedAt);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldDeleteUser()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "deletetest",
            Email = "deletetest@example.com",
            Password = "Password123!"
        };
        var createdUser = await _userService.CreateUserAsync(dto);

        // Act
        var result = await _userService.DeleteUserAsync(createdUser.Id);

        // Assert
        Assert.True(result);

        var deletedUser = await _userService.GetUserByIdAsync(createdUser.Id);
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task GetPagedUsersAsync_ShouldReturnPagedResults()
    {
        // Arrange - 创建多个用户
        for (int i = 0; i < 15; i++)
        {
            await _userService.CreateUserAsync(new CreateUserDto
            {
                Username = $"pageduser{i}",
                Email = $"pageduser{i}@example.com",
                Password = "Password123!"
            });
        }

        // Act
        var (users, total) = await _userService.GetPagedUsersAsync(1, 10);

        // Assert
        Assert.True(total >= 15);
        Assert.True(users.Count <= 10);
    }

    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnTrueForExistingUsername()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "existinguser",
            Email = "existing@example.com",
            Password = "Password123!"
        };
        await _userService.CreateUserAsync(dto);

        // Act
        var exists = await _userService.UsernameExistsAsync("existinguser");

        // Assert
        Assert.True(exists);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
