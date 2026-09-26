using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;

namespace EcomCourse.Application.Carts.Commands.CartCheckout;

public class CartCheckoutCommandHandler : ICommandHandler<CartCheckoutCommand, Guid>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _currentUserService;

    public CartCheckoutCommandHandler(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IUserContext currentUserService)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(
        CartCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        var customerId = _currentUserService.CustomerId;

        var cart = await _cartRepository.GetActiveCartByCustomerIdAsync(customerId, cancellationToken);
        if (cart is null)
            return Result.Failure<Guid>(CartErrors.CartNotFound);

        if (cart.Items is null || cart.Items.Count == 0)
            return Result.Failure<Guid>(CartErrors.CartIsEmpty);

        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var productsDict = await _productRepository.GetProductPricesAsync(productIds, cancellationToken);

        var missing = productIds.Except(productsDict.Keys).ToList();
        if (missing.Count > 0)
            return Result.Failure<Guid>(ProductErrors.Unavailable);

        var items = cart.Items.Select(i => (
            ProductId: i.ProductId,
            Quantity: i.Quantity,
            UnitPrice: productsDict[i.ProductId]
        )).ToList();

        var orderResult = Order.Create(customerId, items);
        if (orderResult.IsFailure)
            return Result.Failure<Guid>(orderResult.Error);

        var order = orderResult.Value!;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _orderRepository.AddAsync(order, cancellationToken);
            cart.Checkout();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(order.Id);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}