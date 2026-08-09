using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EcomCourse.Application.Abstraction.Messaging;

namespace EcomCourse.Application
{
    public record CreateOrderCommand<TResponse>(string ProductName, int Quantity) : ICommand<TResponse>;

    public class CreateOrderHandler<TResponse> : ICommandHandler<CreateOrderCommand<TResponse>, TResponse>
    {
        public async Task<TResponse> HandleAsync(CreateOrderCommand<TResponse> command, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Order created: {command.ProductName} x {command.Quantity}");
            //var order = new Order(command.ProductName, command.Quantity);
            //await _dbContext.Orders.AddAsync(order, cancellationToken);
            //await _dbContext.SaveChangesAsync(cancellationToken);
            await Task.CompletedTask;
            return default!;
        }
    }
}
