using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Orders;

namespace EcomCourse.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductService _productService;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IProductService productService)
    {
        _orderRepository = orderRepository;
        _productService = productService;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var items = new List<(Guid ProductId, int Quantity, decimal UnitPrice)>();

        foreach (var item in request.items)
        {
            var productResult = await _productService.GetByIdAsync(item.ProductId, cancellationToken);

            if (productResult.IsFailure)
            {
                return Result.Failure<Guid>(productResult.Error);
            }

            items.Add((item.ProductId, item.Quantity, productResult.Value!.Amount));
        }

        var orderResult = Order.Create(request.customerId, items);

        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        var order = orderResult.Value!;

        await _orderRepository.AddAsync(order, cancellationToken);

        return Result.Success(order.Id);
    }
}
