using EcomCourse.Application.Carts.Commands.AddItemToCartCommand;
using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;
using Moq;

namespace EcomCourse.UnitTests.CartTest
{
    public class AddItemToCartCommandHandlerTests
    {
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly AddItemToCartCommandHandler _handler;

        public AddItemToCartCommandHandlerTests()
        {
            _cartServiceMock = new Mock<ICartService>();
            _handler = new AddItemToCartCommandHandler(_cartServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenProductNotFound_ReturnsFailure()
        {
            var dto = new AddItemToCartDto { ProductId = Guid.NewGuid(), Quantity = 2 };
            var command = new AddItemToCartCommand(dto);
            var error = new DomainError("Product.NotFound", "Product not found.");

            _cartServiceMock
                .Setup(x => x.AddItemToCartAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(error));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(error.Code, result.Error.Code);

            _cartServiceMock.Verify(
                x => x.AddItemToCartAsync(dto, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_WhenActiveCartAlreadyExists_ReturnsFailure()
        {
            var dto = new AddItemToCartDto { ProductId = Guid.NewGuid(), Quantity = 2 };
            var command = new AddItemToCartCommand(dto);
            var error = new DomainError(
                "Cart.ActiveCartAlreadyExists",
                "Active cart already exists."
            );

            _cartServiceMock
                .Setup(x => x.AddItemToCartAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(error));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal("Cart.ActiveCartAlreadyExists", result.Error.Code);
        }

        [Fact]
        public async Task Handle_WhenAdditionIsSuccessful_ReturnsSuccess()
        {
            var dto = new AddItemToCartDto { ProductId = Guid.NewGuid(), Quantity = 3 };
            var command = new AddItemToCartCommand(dto);

            _cartServiceMock
                .Setup(x => x.AddItemToCartAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);

            _cartServiceMock.Verify(
                x => x.AddItemToCartAsync(dto, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
