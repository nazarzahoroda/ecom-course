using EcomCourse.Application.Abstractions;
using EcomCourse.Application.Services;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;
using Moq;

namespace EcomCourse.Application.Tests.Services;

public class CompensateAsyncTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IIdentityProvider> _identityProviderMock;
    private readonly CompensateAsync _compensateService;

    public CompensateAsyncTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _identityProviderMock = new Mock<IIdentityProvider>();

        _compensateService = new CompensateAsync(
            _customerRepositoryMock.Object,
            _identityProviderMock.Object
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
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _customerRepositoryMock
            .Setup(x => x.DeleteAsync(customer!.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _identityProviderMock
            .Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _compensateService.CompensateAsyncTask(
            userId,
            customerId,
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);

        _customerRepositoryMock.Verify(
            x => x.DeleteAsync(customer!.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _identityProviderMock.Verify(
            x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CompensateAsyncTask_WhenCustomerIdIsEmpty_OnlyDeletesUser()
    {
        var userId = Guid.NewGuid();
        var customerId = Guid.Empty;

        _identityProviderMock
            .Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _compensateService.CompensateAsyncTask(
            userId,
            customerId,
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);

        _customerRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _identityProviderMock.Verify(
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

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _customerRepositoryMock
            .Setup(x => x.DeleteAsync(customer!.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _compensateService.CompensateAsyncTask(
            userId,
            customerId,
            CancellationToken.None
        );

        Assert.True(result.IsFailure);
        Assert.Equal("Compensation.CustomerDeleteFailed", result.Error.Code);

        _identityProviderMock.Verify(
            x => x.DeleteUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CompensateAsyncTask_WhenDeleteUserFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var customerId = Guid.Empty;
        var error = new DomainError(
                        "Identity.DeleteFailed",
                        "Failed to delete user",
                        ErrorType.Conflict);

        _identityProviderMock
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
