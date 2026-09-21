using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Orders;

namespace EcomCourse.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductService _productService;
    private readonly TimeProvider _timeProvider;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductService productService,
        TimeProvider timeProvider)
    {
        _orderRepository = orderRepository;
        _productService = productService;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.items.Select(item => item.ProductId).Distinct().ToList();

        var productsResult = await _productService.GetByIdsAsync(productIds, cancellationToken);

        if (productsResult.IsFailure)
        {
            return Result.Failure<Guid>(productsResult.Error);
        }

        var priceByProductId = productsResult.Value!.ToDictionary(product => product.Id, product => product.Amount);

        var items = request.items
            .Select(item => (item.ProductId, item.Quantity, UnitPrice: priceByProductId[item.ProductId]))
            .ToList();

        var orderResult = Order.Create(request.customerId, items, _timeProvider.GetUtcNow());

        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        var order = orderResult.Value!;

        await _orderRepository.AddAsync(order, cancellationToken);

        return Result.Success(order.Id);
    }
}
