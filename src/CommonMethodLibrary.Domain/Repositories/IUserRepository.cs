using CommonMethodLibrary.Domain.Entities;

namespace CommonMethodLibrary.Domain.Repositories;

/// <summary>
/// 用户仓储接口 - 定义用户特定的查询操作
/// </summary>
public interface IUserRepository : IRepository<User, int>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<(List<User> Users, int Total)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
}
