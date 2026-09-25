using EcomCourse.Application.Authentication.Commands.RegisterCommand;
using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Abstractions;
using EcomCourse.Application.Services;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;
using Moq;

namespace EcomCourse.Application.Tests.Authentication;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IIdentityProvider> _identityProviderMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly CompensateAsync _compensateAsync;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _identityProviderMock = new Mock<IIdentityProvider>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();

        _compensateAsync = new CompensateAsync(
            _customerRepositoryMock.Object,
            _identityProviderMock.Object
        );

        _handler = new RegisterCommandHandler(
            _identityProviderMock.Object,
            _customerRepositoryMock.Object,
            _compensateAsync
        );
    }

    private static RegisterDto CreateValidDto()
    {
        return new RegisterDto
        {
            UserId = Guid.NewGuid(),
            UserName = "ivan_p",
            Name = "Ivan",
            Email = "ivan@example.com",
            Password = "Password123!",
            Street = "Polubotka",
            City = "Lviv",
            PostalCode = "79066",
            Country = "Ukraine",
        };
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ReturnsFailure()
    {
        var dto = CreateValidDto();
        var command = new RegisterCommand(dto);

        _identityProviderMock
            .Setup(x => x.IsUserExist(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Identity.Register", result.Error.Code);

        _identityProviderMock.Verify(
            x =>
                x.CreateUserAsyncWithResult(It.IsAny<RegisterDto>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenUserCreateFails_ReturnsFailure()
    {
        var dto = CreateValidDto();
        var command = new RegisterCommand(dto);
        var error = new DomainError(
                        "Identity.CreationFailed",
                        "Could not create user",
                        ErrorType.Conflict);

        _identityProviderMock
            .Setup(x => x.IsUserExist(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityProviderMock
            .Setup(x => x.CreateUserAsyncWithResult(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ApplicationUserDto>(error));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error.Code, result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenAddingCustomerFails_CallsCompensateAndReturnsFailure()
    {
        var dto = CreateValidDto();
        var command = new RegisterCommand(dto);
        var userDto = new ApplicationUserDto { Id = dto.UserId!.Value, Email = dto.Email };

        _identityProviderMock
            .Setup(x => x.IsUserExist(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityProviderMock
            .Setup(x => x.CreateUserAsyncWithResult(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(userDto));

        _customerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityProviderMock
            .Setup(x => x.DeleteUserAsync(dto.UserId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Customer.CreateFailed", result.Error.Code);

        _identityProviderMock.Verify(
            x => x.DeleteUserAsync(dto.UserId.Value, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenAllStepsSucceed_ReturnsSuccess()
    {
        var dto = CreateValidDto();
        var command = new RegisterCommand(dto);
        var userDto = new ApplicationUserDto { Id = dto.UserId!.Value, Email = dto.Email };

        _identityProviderMock
            .Setup(x => x.IsUserExist(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityProviderMock
            .Setup(x => x.CreateUserAsyncWithResult(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(userDto));

        _customerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _identityProviderMock
            .Setup(x =>
                x.SetCustomerIdAsync(
                    dto.UserId.Value,
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Success());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        _identityProviderMock.Verify(
            x =>
                x.SetCustomerIdAsync(
                    dto.UserId.Value,
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _identityProviderMock.Verify(
            x => x.DeleteUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
