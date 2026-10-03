using EcomCourse.Application.Carts.Commands.AddItemToCartCommand;
using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;
using Moq;

namespace EcomCourse.UnitTests.CartTest
{
    public class AddItemToCartCommandHandlerTests
    {
        private readonly Mock<ICartManager> _cartManagerMock;
        private readonly AddItemToCartCommandHandler _handler;

        public AddItemToCartCommandHandlerTests()
        {
            _cartManagerMock = new Mock<ICartManager>();
            _handler = new AddItemToCartCommandHandler(_cartManagerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenProductNotFound_ReturnsFailure()
        {
            var dto = new AddItemToCartDto { ProductId = Guid.NewGuid(), Quantity = 2 };
            var command = new AddItemToCartCommand(dto);
            var error = new DomainError(
                            "Product.NotFound",
                            "Product not found.",
                            ErrorType.NotFound);

            _cartManagerMock
                .Setup(x => x.AddItemToCartAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(error));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(error.Code, result.Error.Code);

            _cartManagerMock.Verify(
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
                            "Active cart already exists.",
                            ErrorType.Conflict);

            _cartManagerMock
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

            _cartManagerMock
                .Setup(x => x.AddItemToCartAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);

            _cartManagerMock.Verify(
                x => x.AddItemToCartAsync(dto, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
