using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;

namespace EcomCourse.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerStore _customerStore;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, ICustomerStore customerStore)
    {
        _orderRepository = orderRepository;
        _customerStore = customerStore;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var items = request.items
            .Select(i => (i.ProductId, i.Quantity, i.UnitPrice))
            .ToList();

        if (items.Count == 0)
        {
            return Result.Failure<Guid>(OrderErrors.EmptyLines);
        }

        var customer = await _customerStore.GetByIdAsync(request.customerId, cancellationToken);
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

        var orderResult = Order.Create(request.customerId, shippingAddressResult.Value!, items);

        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        var order = orderResult.Value!;

        await _orderRepository.AddAsync(order, cancellationToken);

        return Result.Success(order.Id);
    }
}
