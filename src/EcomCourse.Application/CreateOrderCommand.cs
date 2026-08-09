
using EcomCourse.Application.Abstraction.Messaging;

namespace EcomCourse.Application
{
    public record CreateOrderCommand<TResponse>(string ProductName, int Quantity) : ICommand<TResponse>;

    public class CreateOrderHandler<TResponse> : ICommandHandler<CreateOrderCommand<TResponse>, TResponse>
    {
        public async Task<TResponse> HandleAsync(CreateOrderCommand<TResponse> command, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Order created: {command.ProductName} x {command.Quantity}");
            await Task.CompletedTask;
            return default!;
        }
    }
}
