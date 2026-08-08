using System;
using System.Collections.Generic;
using System.Text;

namespace EcomCourse.Application
{
    internal interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command, CancellationToken cancellationToken);
    }
}
