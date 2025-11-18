using CommonMethodLibrary.Application.Common;
using CommonMethodLibrary.Application.Users.DTOs;
using CommonMethodLibrary.Domain.Repositories;
using MediatR;

namespace CommonMethodLibrary.Application.Users.Queries;

/// <summary>
/// 分页获取用户查询
/// </summary>
public record GetPagedUsersQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? SearchTerm = null
) : IQuery<Result<PagedResult<UserDto>>>;

/// <summary>
/// 分页结果
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}

/// <summary>
/// 分页获取用户查询处理器
/// </summary>
public class GetPagedUsersQueryHandler : IRequestHandler<GetPagedUsersQuery, Result<PagedResult<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetPagedUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetPagedUsersQuery request, CancellationToken cancellationToken)
    {
        var (users, total) = await _userRepository.GetPagedAsync(
            request.PageIndex,
            request.PageSize,
            request.SearchTerm,
            cancellationToken
        );

        var dtos = users.Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email.Value,
            PhoneNumber = user.PhoneNumber?.Value,
            RealName = user.RealName,
            Age = user.Age,
            IsActive = user.IsActive,
            Status = user.Status.ToString(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt
        }).ToList();

        var result = new PagedResult<UserDto>
        {
            Items = dtos,
            Total = total,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };

        return Result<PagedResult<UserDto>>.Success(result);
    }
}
