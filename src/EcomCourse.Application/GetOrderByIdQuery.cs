using System;
using System.Collections.Generic;
using System.Text;
using EcomCourse.Application.Abstraction.Messaging;

namespace EcomCourse.Application
{
    public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto>;

    public record OrderDto(Guid Id, string ProductName, int Quantity);

    public class GetOrderByIdHandler : IQueryHandler<GetOrderByIdQuery, OrderDto>
    {
        public async Task<OrderDto> HandleAsync(GetOrderByIdQuery query, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new OrderDto(query.OrderId, "Laptop", 2));
        }
    }
}

//// 1. Створюємо команду, де TResponse це Guid (ID замовлення)
//var command = new CreateOrderCommand<Guid>("Ноутбук", 2);

//// 2. Медіатор або DI-контейнер знаходить відповідний обробник і викликає метод
//var handler = new CreateOrderHandler<Guid>();
//Guid orderId = await handler.HandleAsync(command, CancellationToken.None);
