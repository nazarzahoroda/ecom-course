using System;
using System.Collections.Generic;
using System.Text;

namespace EcomCourse.Application.Abstraction.Messaging
{
    public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
    {
        Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
    }
}
