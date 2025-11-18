using CommonMethodLibrary.Application.Common;
using CommonMethodLibrary.Application.Users.DTOs;
using CommonMethodLibrary.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace CommonMethodLibrary.Application.Users.Commands;

/// <summary>
/// 更新用户命令
/// </summary>
public record UpdateUserCommand(
    int Id,
    string? Username,
    string? Email,
    string? PhoneNumber,
    string? RealName,
    int? Age
) : ICommand<Result<UserDto>>;

/// <summary>
/// 更新用户命令验证器
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("用户ID必须大于0");

        RuleFor(x => x.Age)
            .InclusiveBetween(1, 150).When(x => x.Age.HasValue)
            .WithMessage("年龄必须在1-150之间");
    }
}

/// <summary>
/// 更新用户命令处理器
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure("用户不存在");
        }

        // 使用领域方法更新用户
        user.UpdateProfile(
            request.Username,
            request.Email,
            request.PhoneNumber,
            request.RealName,
            request.Age
        );

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(user);
        return Result<UserDto>.Success(dto);
    }

    private static UserDto MapToDto(Domain.Entities.User user)
    {
        return new UserDto
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
    }
}
