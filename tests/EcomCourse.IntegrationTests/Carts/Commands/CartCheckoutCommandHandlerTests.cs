using EcomCourse.Application.Carts.Commands.CartCheckout;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Orders;
using EcomCourse.Domain.Products;
using FluentAssertions;
using Moq;
using Xunit;

namespace EcomCourse.UnitTests.Carts.Commands;

public class CartCheckoutCommandHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly CartCheckoutCommandHandler _handler;

    private readonly Guid _customerId = Guid.NewGuid();

    public CartCheckoutCommandHandlerTests()
    {
        _cartRepositoryMock = new Mock<ICartRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userContextMock = new Mock<IUserContext>();

        _userContextMock.Setup(u => u.CustomerId).Returns(_customerId);

        _handler = new CartCheckoutCommandHandler(
            _cartRepositoryMock.Object,
            _productRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCartNotFound()
    {
        // Arrange
        _cartRepositoryMock
            .Setup(x => x.GetActiveCartByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart)null!);

        // Act
        var result = await _handler.Handle(new CartCheckoutCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CartErrors.CartNotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCartIsEmpty()
    {
        // Arrange
        var cart = new Cart(Guid.NewGuid(), _customerId);
        _cartRepositoryMock
            .Setup(x => x.GetActiveCartByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        // Act
        var result = await _handler.Handle(new CartCheckoutCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CartErrors.CartIsEmpty);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductsAreMissing()
    {
        // Arrange
        var cart = new Cart(Guid.NewGuid(), _customerId);
        cart.AddItem(Guid.NewGuid(), 1);

        _cartRepositoryMock
            .Setup(x => x.GetActiveCartByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _productRepositoryMock
            .Setup(x => x.GetProductPricesAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, decimal>());

        // Act
        var result = await _handler.Handle(new CartCheckoutCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.Unavailable);
    }

    [Fact]
    public async Task Handle_ShouldCommitTransaction_WhenSuccessful()
    {
        // Arrange
        var cart = new Cart(Guid.NewGuid(), _customerId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 2);

        _cartRepositoryMock
            .Setup(x => x.GetActiveCartByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var prices = new Dictionary<Guid, decimal> { { productId, 100m } };
        _productRepositoryMock
            .Setup(x => x.GetProductPricesAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(prices);

        // Act
        var result = await _handler.Handle(new CartCheckoutCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRollbackTransaction_WhenExceptionThrown()
    {
        // Arrange
        var cart = new Cart(Guid.NewGuid(), _customerId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 2);

        _cartRepositoryMock
            .Setup(x => x.GetActiveCartByCustomerIdAsync(_customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var prices = new Dictionary<Guid, decimal> { { productId, 100m } };
        _productRepositoryMock
            .Setup(x => x.GetProductPricesAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(prices);

        // Використовуємо конкретне виключення замість загального System.Exception
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var action = async () => await _handler.Handle(new CartCheckoutCommand(), CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Database error");

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
