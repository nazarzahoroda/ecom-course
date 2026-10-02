using EcomCourse.Application.Abstractions;
using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Exceptions;
using EcomCourse.Application.Products;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;

namespace EcomCourse.Application.Carts.Commands.CartCheckout;

public class CartCheckoutCommandHandler : ICommandHandler<CartCheckoutCommand, Guid>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductManager _productManager;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _currentUserService;
    private readonly ICustomerRepository _customerRepository;

    public CartCheckoutCommandHandler(
        ICartRepository cartRepository,
        IProductManager productManager,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IUserContext currentUserService,
        ICustomerRepository customerRepository)
    {
        _cartRepository = cartRepository;
        _productManager = productManager;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _customerRepository = customerRepository;
    }

    public async Task<Result<Guid>> Handle(
        CartCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        var customerId = _currentUserService.CustomerId;

        var customer = await _customerRepository.GetByIdAsync(
            customerId,
            cancellationToken);

        if (customer is null)
        {
            return Result.Failure<Guid>(CustomerErrors.NotFound);
        }

        var cart = await _cartRepository.GetActiveCartByCustomerIdAsync(
            customerId,
            cancellationToken);

        if (cart is null)
        {
            return Result.Failure<Guid>(CartErrors.CartNotFound);
        }

        if (cart.Items is null || cart.Items.Count == 0)
        {
            return Result.Failure<Guid>(CartErrors.CartIsEmpty);
        }

        var productIds = cart.Items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var productsResult = await _productManager.GetByIdsAsync(
            productIds,
            cancellationToken);

        if (productsResult.IsFailure)
        {
            return Result.Failure<Guid>(productsResult.Error);
        }

        var productsById = productsResult.Value!
            .ToDictionary(product => product.Id);

        var missing = productIds
            .Except(productsById.Keys)
            .ToList();

        if (missing.Count > 0)
        {
            return Result.Failure<Guid>(ProductErrors.Unavailable);
        }

        var items = cart.Items
            .Select(i =>
                (
                    ProductId: i.ProductId,
                    Quantity: i.Quantity,
                    UnitPrice: productsById[i.ProductId].Amount,
                    Currency: productsById[i.ProductId].Currency
                ))
            .ToList();

        var orderResult = Order.Create(
            customerId,
            customer.Address,
            items,
            DateTime.UtcNow);

        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        var order = orderResult.Value!;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _orderRepository.AddAsync(order, cancellationToken);

            cart.Checkout();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Failure<Guid>(CartErrors.CartNotActive);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }

        return Result.Success(order.Id);
    }
}
