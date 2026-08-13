using EcomCourse.Domain.Common;
using MediatR;

namespace Gatherly.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
