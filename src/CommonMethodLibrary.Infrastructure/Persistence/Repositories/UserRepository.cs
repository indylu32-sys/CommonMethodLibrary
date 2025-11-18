using CommonMethodLibrary.Domain.Entities;
using CommonMethodLibrary.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CommonMethodLibrary.Infrastructure.Persistence.Repositories;

/// <summary>
/// 用户仓储实现
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.ToListAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        _context.Users.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<(List<User> Users, int Total)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.Username.Contains(searchTerm) ||
                u.Email.Value.Contains(searchTerm) ||
                (u.RealName != null && u.RealName.Contains(searchTerm)));
        }

        var total = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, total);
    }
}
