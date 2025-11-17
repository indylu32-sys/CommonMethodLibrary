using CommonMethodLibrary.WebAPI.Models.DTOs;
using CommonMethodLibrary.WebAPI.Models.Entities;

namespace CommonMethodLibrary.WebAPI.Services;

/// <summary>
/// 用户服务接口
/// </summary>
public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User> CreateUserAsync(CreateUserDto dto);
    Task<User?> UpdateUserAsync(int id, UpdateUserDto dto);
    Task<bool> DeleteUserAsync(int id);
    Task<(List<User> users, int total)> GetPagedUsersAsync(int pageIndex, int pageSize, string? searchTerm = null);
    Task<bool> UserExistsAsync(int id);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}
