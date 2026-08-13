using EcomCourse.Domain.Common;
using MediatR;

namespace Gatherly.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
