using MediatR;

namespace CommonMethodLibrary.Application.Common;

/// <summary>
/// 命令接口 - CQRS模式中的命令（写操作）
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// 无返回值的命令接口
/// </summary>
public interface ICommand : IRequest
{
}
