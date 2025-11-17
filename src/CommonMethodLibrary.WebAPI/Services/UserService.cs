using CommonMethodLibrary.Core.Helpers;
using CommonMethodLibrary.WebAPI.Data;
using CommonMethodLibrary.WebAPI.Models.DTOs;
using CommonMethodLibrary.WebAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommonMethodLibrary.WebAPI.Services;

/// <summary>
/// 用户服务实现
/// 演示如何在服务层使用工具库
/// </summary>
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly CacheHelper _cache;

    public UserService(
        ApplicationDbContext context,
        ILogger<UserService> logger,
        CacheHelper cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        // 演示使用缓存
        return await _cache.GetOrSetAsync(
            "all_users",
            async () => await _context.Users.ToListAsync(),
            TimeSpan.FromMinutes(5)
        );
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        var cacheKey = $"user_{id}";
        return await _cache.GetOrSetAsync(
            cacheKey,
            async () => await _context.Users.FindAsync(id),
            TimeSpan.FromMinutes(10)
        );
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User> CreateUserAsync(CreateUserDto dto)
    {
        // 演示使用加密工具
        var passwordHash = EncryptionHelper.Sha256(dto.Password);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = passwordHash,
            RealName = dto.RealName,
            Age = dto.Age,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 清除缓存
        _cache.Remove("all_users");

        _logger.LogInformation("创建用户: {Username}, ID: {Id}, 时间戳: {Timestamp}",
            user.Username, user.Id, DateTimeHelper.GetCurrentTimestamp());

        return user;
    }

    public async Task<User?> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return null;

        // 只更新提供的字段
        if (!StringHelper.IsNullOrWhiteSpace(dto.Username))
            user.Username = dto.Username;

        if (!StringHelper.IsNullOrWhiteSpace(dto.Email))
            user.Email = dto.Email;

        if (dto.PhoneNumber != null)
            user.PhoneNumber = dto.PhoneNumber;

        if (dto.RealName != null)
            user.RealName = dto.RealName;

        if (dto.Age.HasValue)
            user.Age = dto.Age.Value;

        if (dto.IsActive.HasValue)
            user.IsActive = dto.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // 清除相关缓存
        _cache.Remove($"user_{id}");
        _cache.Remove("all_users");

        _logger.LogInformation("更新用户: {Username}, ID: {Id}", user.Username, user.Id);

        return user;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // 清除缓存
        _cache.Remove($"user_{id}");
        _cache.Remove("all_users");

        _logger.LogInformation("删除用户: {Username}, ID: {Id}", user.Username, user.Id);

        return true;
    }

    public async Task<(List<User> users, int total)> GetPagedUsersAsync(int pageIndex, int pageSize, string? searchTerm = null)
    {
        var query = _context.Users.AsQueryable();

        if (!StringHelper.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.Username.Contains(searchTerm) ||
                u.Email.Contains(searchTerm) ||
                (u.RealName != null && u.RealName.Contains(searchTerm)));
        }

        var total = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, total);
    }

    public async Task<bool> UserExistsAsync(int id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
}
