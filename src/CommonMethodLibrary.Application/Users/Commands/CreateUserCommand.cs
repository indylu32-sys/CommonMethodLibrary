using CommonMethodLibrary.Application.Common;
using CommonMethodLibrary.Application.Users.DTOs;
using CommonMethodLibrary.Core.Helpers;
using CommonMethodLibrary.Domain.Entities;
using CommonMethodLibrary.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace CommonMethodLibrary.Application.Users.Commands;

/// <summary>
/// 创建用户命令
/// </summary>
public record CreateUserCommand(
    string Username,
    string Email,
    string? PhoneNumber,
    string Password,
    string? RealName,
    int? Age
) : ICommand<Result<UserDto>>;

/// <summary>
/// 创建用户命令验证器
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .Length(3, 50).WithMessage("用户名长度必须在3-50个字符之间");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空")
            .Must(email => ValidationHelper.IsValidEmail(email)).WithMessage("邮箱格式不正确");

        RuleFor(x => x.PhoneNumber)
            .Must(phone => string.IsNullOrEmpty(phone) || ValidationHelper.IsValidPhoneNumber(phone))
            .WithMessage("手机号格式不正确");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .Must(pwd => ValidationHelper.IsStrongPassword(pwd))
            .WithMessage("密码必须至少8位，包含大小写字母、数字和特殊字符");

        RuleFor(x => x.Age)
            .InclusiveBetween(1, 150).When(x => x.Age.HasValue)
            .WithMessage("年龄必须在1-150之间");
    }
}

/// <summary>
/// 创建用户命令处理器
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 检查用户名是否已存在
        if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken))
        {
            return Result<UserDto>.Failure("用户名已存在");
        }

        // 检查邮箱是否已存在
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result<UserDto>.Failure("邮箱已存在");
        }

        // 使用加密工具生成密码哈希
        var passwordHash = EncryptionHelper.Sha256(request.Password);

        // 创建用户聚合根
        var user = User.Create(
            request.Username,
            request.Email,
            passwordHash,
            request.PhoneNumber,
            request.RealName,
            request.Age
        );

        // 保存到仓储
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 返回结果
        var dto = MapToDto(user);
        return Result<UserDto>.Success(dto);
    }

    private static UserDto MapToDto(User user)
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
