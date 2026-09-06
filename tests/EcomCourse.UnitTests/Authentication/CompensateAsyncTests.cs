using EcomCourse.Application.Interfaces;
using EcomCourse.Application.Services;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;
using Moq;

namespace EcomCourse.Application.Tests.Services;

public class CompensateAsyncTests
{
    private readonly Mock<ICustomerStore> _customerStoreMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly CompensateAsync _compensateService;

    public CompensateAsyncTests()
    {
        _customerStoreMock = new Mock<ICustomerStore>();
        _identityServiceMock = new Mock<IIdentityService>();

        _compensateService = new CompensateAsync(
            _customerStoreMock.Object,
            _identityServiceMock.Object
        );
    }

    [Fact]
    public async Task CompensateAsyncTask_WhenCustomerExistsAndDeletedSuccessfully_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customerResult = Customer.Create(
            userId,
            "Ivan",
            "ivan@example.com",
            "Polubotka",
            "Lviv",
            "79066",
            "Ukraine"
        );
        var customer = customerResult.Value;
        _customerStoreMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _customerStoreMock
            .Setup(x => x.DeleteAsync(customer!.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _identityServiceMock
            .Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _compensateService.CompensateAsyncTask(
            userId,
            customerId,
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);

        _customerStoreMock.Verify(
            x => x.DeleteAsync(customer!.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _identityServiceMock.Verify(
            x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CompensateAsyncTask_WhenCustomerIdIsEmpty_OnlyDeletesUser()
    {
        var userId = Guid.NewGuid();
        var customerId = Guid.Empty;

        _identityServiceMock
            .Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _compensateService.CompensateAsyncTask(
            userId,
            customerId,
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);

        _customerStoreMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _identityServiceMock.Verify(
            x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CompensateAsyncTask_WhenDeleteCustomerFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customerResult = Customer.Create(
            userId,
            "Ivan",
            "ivan@example.com",
            "Polubotka",
            "Lviv",
            "79066",
            "Ukraine"
        );

        var customer = customerResult.Value;

        _customerStoreMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _customerStoreMock
            .Setup(x => x.DeleteAsync(customer!.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _compensateService.CompensateAsyncTask(
            userId,
            customerId,
            CancellationToken.None
        );

        Assert.True(result.IsFailure);
        Assert.Equal("Compensation.CustomerDeleteFailed", result.Error.Code);

        _identityServiceMock.Verify(
            x => x.DeleteUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CompensateAsyncTask_WhenDeleteUserFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var customerId = Guid.Empty;
        var error = new DomainError("Identity.DeleteFailed", "Failed to delete user");

        _identityServiceMock
            .Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

        var result = await _compensateService.CompensateAsyncTask(
            userId,
            customerId,
            CancellationToken.None
        );

        Assert.True(result.IsFailure);
        Assert.Equal(error.Code, result.Error.Code);
    }
}
