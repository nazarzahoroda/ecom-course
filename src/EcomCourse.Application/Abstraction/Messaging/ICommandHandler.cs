using System;
using System.Collections.Generic;
using System.Text;

namespace EcomCourse.Application.Abstraction.Messaging
{
    public interface ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
    {
        Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken);
    }
}
