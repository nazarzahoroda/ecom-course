using EcomCourse.Application.Orders.Queries.GetOrderWithLines;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Orders;
using NSubstitute;
using Xunit;

namespace EcomCourse.UnitTests;

public class GetOrderWithLinesQueryHandlerTests
{
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly GetOrderWithLinesQueryHandler _handler;

    public GetOrderWithLinesQueryHandlerTests()
    {
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _handler = new GetOrderWithLinesQueryHandler(_orderRepositoryMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWithNotFoundError_WhenOrderDoesNotExist()
    {
        // Arrange
        var query = new GetOrderWithLinesQuery(Guid.NewGuid());

        // Мокаємо репозиторій так, щоб він повертав null
        _orderRepositoryMock.GetByIdAsync(query.orderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.NotFound, result.Error);
    }
}
