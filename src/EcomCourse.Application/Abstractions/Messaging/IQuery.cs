using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
