using EcomCourse.Application.Abstractions;
using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Products;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;

namespace EcomCourse.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductManager _productManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductManager productManager,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork
    )
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productManager = productManager;
        _timeProvider = timeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        if (request.items.Count == 0)
        {
            return Result.Failure<Guid>(OrderErrors.EmptyLines);
        }

        var productIds = request.items
            .Select(item => item.ProductId)
            .Distinct()
            .ToList();

        var productsResult = await _productManager.GetByIdsAsync(
            productIds,
            cancellationToken
        );

        if (productsResult.IsFailure)
        {
            return Result.Failure<Guid>(productsResult.Error);
        }

        var productsById = productsResult.Value!
            .ToDictionary(product => product.Id);

        var items = request.items
            .Select(i =>
                (
                    ProductId: i.ProductId,
                    Quantity: i.Quantity,
                    UnitPrice: productsById[i.ProductId].Amount,
                    Currency: productsById[i.ProductId].Currency
                )
            )
            .ToList();

        var customer = await _customerRepository.GetByIdAsync(
            request.customerId,
            cancellationToken
        );

        if (customer is null)
        {
            return Result.Failure<Guid>(CustomerErrors.NotFound);
        }

        var shippingAddressResult = Address.Create(
            customer.Address.Street,
            customer.Address.City,
            customer.Address.PostalCode,
            customer.Address.Country
        );

        if (shippingAddressResult.IsFailure)
        {
            return Result.Failure<Guid>(shippingAddressResult.Error);
        }

        var orderResult = Order.Create(
            request.customerId,
            shippingAddressResult.Value!,
            items,
            _timeProvider.GetUtcNow()
        );

        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        var order = orderResult.Value!;

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Id);
    }


}
