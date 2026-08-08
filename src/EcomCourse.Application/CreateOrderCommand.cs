using System;
using System.Collections.Generic;
using System.Text;

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
