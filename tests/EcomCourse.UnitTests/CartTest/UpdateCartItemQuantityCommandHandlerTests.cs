using EcomCourse.Application.Carts.Commands.UpdateCartItemQuantityCommand;
using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;
using Moq;

namespace EcomCourse.UnitTests.CartTest
{
    public class UpdateCartItemQuantityCommandHandlerTests
    {
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly UpdateCartItemQuantityCommandHandler _handler;

        public UpdateCartItemQuantityCommandHandlerTests()
        {
            _cartServiceMock = new Mock<ICartService>();
            _handler = new UpdateCartItemQuantityCommandHandler(_cartServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenItemNotFound_ReturnsFailure()
        {
            var dto = new UpdateCartItemQuantityDto { ProductId = Guid.NewGuid(), Quantity = 2 };
            var command = new UpdateCartItemQuantityCommand(dto);
            var error = new DomainError("CartItem.NotFound", "Item not found.");

            _cartServiceMock
                .Setup(x => x.UpdateCartItemQuantityAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(error));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(error.Code, result.Error.Code);

            _cartServiceMock.Verify(
                x => x.UpdateCartItemQuantityAsync(dto, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_WhenUpdateIsSuccessful_ReturnsSuccess()
        {
            var dto = new UpdateCartItemQuantityDto { ProductId = Guid.NewGuid(), Quantity = 2 };
            var command = new UpdateCartItemQuantityCommand(dto);

            _cartServiceMock
                .Setup(x => x.UpdateCartItemQuantityAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);

            _cartServiceMock.Verify(
                x => x.UpdateCartItemQuantityAsync(dto, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
