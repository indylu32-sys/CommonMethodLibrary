using CommonMethodLibrary.Application.Common;
using CommonMethodLibrary.Domain.Repositories;
using MediatR;

namespace CommonMethodLibrary.Application.Users.Commands;

/// <summary>
/// 删除用户命令
/// </summary>
public record DeleteUserCommand(int Id) : ICommand<Result>;

/// <summary>
/// 删除用户命令处理器
/// </summary>
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
        {
            return Result.Failure("用户不存在");
        }

        await _userRepository.DeleteAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
