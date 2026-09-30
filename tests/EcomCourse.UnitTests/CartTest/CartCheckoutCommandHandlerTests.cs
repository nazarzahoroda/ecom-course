using EcomCourse.Application.Carts.Commands.CartCheckout;
using EcomCourse.Application.Interfaces;
using EcomCourse.Application.Products;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;
using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;
using Moq;
using Xunit;

namespace EcomCourse.UnitTests.Carts.Commands;

public class CartCheckoutCommandHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<ICustomerStore> _customerStoreMock;
    private readonly CartCheckoutCommandHandler _handler;

    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Customer _customer;

    public CartCheckoutCommandHandlerTests()
    {
        _cartRepositoryMock = new Mock<ICartRepository>();
        _productServiceMock = new Mock<IProductService>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userContextMock = new Mock<IUserContext>();
        _customerStoreMock = new Mock<ICustomerStore>();

        _userContextMock.Setup(u => u.CustomerId).Returns(_customerId);

        var addressResult = Address.Create("Main St 1", "Kyiv", "01001", "Ukraine");
        var customerResult = Customer.Create(
            Guid.NewGuid(),
            "John Doe",
            "john@example.com",
            "Main St 1",
            "Kyiv",
            "01001",
            "Ukraine"
        );
        _customer = customerResult.Value!;

        // За замовчуванням повертаємо покупця
        _customerStoreMock
            .Setup(x => x.GetByIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_customer);

        _handler = new CartCheckoutCommandHandler(
            _cartRepositoryMock.Object,
            _productServiceMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object,
            _customerStoreMock.Object
        );
    }

    private static ProductDto CreateTestProductDto(Guid productId, decimal amount = 100m)
    {
        return new ProductDto(
            Id: productId,
            Name: "Test Product",
            Amount: amount,
            Currency: Currency.USD, // або Currency.Create("USD").Value, якщо це Value Object
            SKU: "TEST-SKU-001",
            CategoryId: Guid.NewGuid()
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCustomerNotFound()
    {
        _customerStoreMock
            .Setup(x => x.GetByIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer)null!);

        var result = await _handler.Handle(new CartCheckoutCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCartNotFound()
    {
        _cartRepositoryMock
            .Setup(x =>
                x.GetActiveCartByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Cart)null!);

        var result = await _handler.Handle(new CartCheckoutCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CartErrors.CartNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCartIsEmpty()
    {
        var cart = Cart.Create(_customerId);

        _cartRepositoryMock
            .Setup(x =>
                x.GetActiveCartByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(cart.Value);

        var result = await _handler.Handle(new CartCheckoutCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CartErrors.CartIsEmpty, result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductsAreMissing()
    {
        var cartResult = Cart.Create(_customerId);
        var cart = cartResult.Value!;
        cart.AddItem(Guid.NewGuid(), 1);

        _cartRepositoryMock
            .Setup(x =>
                x.GetActiveCartByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(cart);

        _productServiceMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<ProductDto>>(new List<ProductDto>()));

        var result = await _handler.Handle(new CartCheckoutCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.Unavailable, result.Error);
    }

    [Fact]
    public async Task Handle_ShouldCommitTransaction_WhenSuccessful()
    {
        var cart = Cart.Create(_customerId);
        var productId = Guid.NewGuid();
        cart.Value!.AddItem(productId, 2);

        _cartRepositoryMock
            .Setup(x =>
                x.GetActiveCartByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(cart.Value);

        var productDto = CreateTestProductDto(productId, 100m);
        _productServiceMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Success<IReadOnlyList<ProductDto>>(new List<ProductDto> { productDto })
            );

        var result = await _handler.Handle(new CartCheckoutCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        _orderRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldRollbackTransaction_WhenExceptionThrown()
    {
        var cart = Cart.Create(_customerId);
        var productId = Guid.NewGuid();
        cart.Value!.AddItem(productId, 2);

        _cartRepositoryMock
            .Setup(x =>
                x.GetActiveCartByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(cart.Value);

        var productDto = CreateTestProductDto(productId, 100m);
        _productServiceMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Success<IReadOnlyList<ProductDto>>(new List<ProductDto> { productDto })
            );

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new CartCheckoutCommand(), CancellationToken.None)
        );

        Assert.Equal("Database error", exception.Message);

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(CancellationToken.None), Times.Once);
    }
}
