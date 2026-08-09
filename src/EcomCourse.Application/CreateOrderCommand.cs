<<<<<<< HEAD

using EcomCourse.Application.Abstraction.Messaging;
=======
using System;
using System.Collections.Generic;
using System.Text;
>>>>>>> parent of be43481 (fixed cqrs-pattern)

namespace EcomCourse.Application
{
    public record CreateOrderCommand(string ProductName, int Quantity) : ICommand;

    public class CreateOrderHandler : ICommandHandler<CreateOrderCommand>
    {
        public async Task HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Order created: {command.ProductName} x {command.Quantity}");
            await Task.CompletedTask;
        }
    }
}
