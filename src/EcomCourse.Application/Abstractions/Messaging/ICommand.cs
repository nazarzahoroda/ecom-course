using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
