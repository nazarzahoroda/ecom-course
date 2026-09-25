using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Products;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;

namespace EcomCourse.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductManager _productManager;
    private readonly TimeProvider _timeProvider;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductManager productManager,
        TimeProvider timeProvider)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productManager = productManager;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.items.Select(item => item.ProductId).Distinct().ToList();

        var productsResult = await _productManager.GetByIdsAsync(productIds, cancellationToken);

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

        if (items.Count == 0)
        {
            return Result.Failure<Guid>(OrderErrors.EmptyLines);
        }

        var customer = await _customerRepository.GetByIdAsync(request.customerId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<Guid>(CustomerErrors.NotFound);
        }

        var shippingAddressResult = Address.Create(
            customer.Address.Street,
            customer.Address.City,
            customer.Address.PostalCode,
            customer.Address.Country);
        if (shippingAddressResult.IsFailure)
        {
            return Result.Failure<Guid>(shippingAddressResult.Error);
        }

        var orderResult = Order.Create(
            request.customerId,
            shippingAddressResult.Value!,
            items,
            _timeProvider.GetUtcNow());

        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        var order = orderResult.Value!;

        await _orderRepository.AddAsync(order, cancellationToken);

        return Result.Success(order.Id);
    }
}
