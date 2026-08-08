using System;
using System.Collections.Generic;
using System.Text;
using EcomCourse.Application;

namespace EcomCourse.Application
{
    public record GetOrderByIdQuery(Guid OrderId) : IQuery<GetOrderByIdQuery, OrderDto>;

    public record OrderDto(Guid Id, string ProductName, int Quantity);

    public class GetOrderByIdHandler : IQueryHandler<GetOrderByIdQuery, OrderDto>
    {
        public async Task<OrderDto> HandleAsync(GetOrderByIdQuery query, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new OrderDto(query.OrderId, "Laptop", 2));
        }
    }
}

