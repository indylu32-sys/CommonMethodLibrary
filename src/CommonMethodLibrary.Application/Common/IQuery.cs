using MediatR;

namespace CommonMethodLibrary.Application.Common;

/// <summary>
/// 查询接口 - CQRS模式中的查询（读操作）
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
