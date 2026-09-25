using EcomCourse.Application.Carts.Commands.RemoveItemFromCartCommand;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;
using Moq;

namespace EcomCourse.UnitTests.CartTest
{
    public class RemoveItemFromCartCommandHandlerTests
    {
        private readonly Mock<ICartManager> _cartManagerMock;
        private readonly RemoveItemFromCartCommandHandler _handler;

        public RemoveItemFromCartCommandHandlerTests()
        {
            _cartManagerMock = new Mock<ICartManager>();
            _handler = new RemoveItemFromCartCommandHandler(_cartManagerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenItemNotFound_ReturnsFailure()
        {
            var itemId = Guid.NewGuid();
            var command = new RemoveItemFromCartCommand(itemId);
            var error = new DomainError(
                            "CartItem.NotFound",
                            "Cart item not found.",
                            ErrorType.NotFound);

            _cartManagerMock
                .Setup(x => x.RemoveItemFromCartAsync(itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<Guid>(error));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(error.Code, result.Error.Code);
            Assert.Equal(error.Description, result.Error.Description);

            _cartManagerMock.Verify(
                x => x.RemoveItemFromCartAsync(itemId, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_WhenRemovalIsSuccessful_ReturnsSuccessWithId()
        {
            var itemId = Guid.NewGuid();
            var command = new RemoveItemFromCartCommand(itemId);

            _cartManagerMock
                .Setup(x => x.RemoveItemFromCartAsync(itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(itemId));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(itemId, result.Value);

            _cartManagerMock.Verify(
                x => x.RemoveItemFromCartAsync(itemId, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
