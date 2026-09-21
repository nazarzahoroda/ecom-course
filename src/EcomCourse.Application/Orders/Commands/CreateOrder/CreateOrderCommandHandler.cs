using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Products;
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
        var productIds = request.items.Select(item => item.ProductId).Distinct().ToList();

        var productsResult = await _productService.GetByIdsAsync(productIds, cancellationToken);

        if (productsResult.IsFailure)
        {
            return Result.Failure<Guid>(productsResult.Error);
        }

        var productsById = productsResult.Value!.ToDictionary(product => product.Id);

        // Server derives UnitPrice and Currency from the product's own price —
        // never trust these financial fields from the client (see docs/review-rules.md).
        var items = request.items
            .Select(i => (
                i.ProductId,
                i.Quantity,
                UnitPrice: productsById[i.ProductId].Amount,
                Currency: productsById[i.ProductId].Currency))
            .ToList();

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
