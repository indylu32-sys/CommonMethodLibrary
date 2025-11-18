using CommonMethodLibrary.Application.Common;
using CommonMethodLibrary.Application.Users.DTOs;
using CommonMethodLibrary.Domain.Repositories;
using MediatR;

namespace CommonMethodLibrary.Application.Users.Queries;

/// <summary>
/// 根据ID获取用户查询
/// </summary>
public record GetUserByIdQuery(int Id) : IQuery<Result<UserDto>>;

/// <summary>
/// 获取用户查询处理器
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure("用户不存在");
        }

        var dto = new UserDto
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
        };

        return Result<UserDto>.Success(dto);
    }
}
