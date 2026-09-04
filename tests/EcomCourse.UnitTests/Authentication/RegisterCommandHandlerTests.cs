using EcomCourse.Application.Authentication.Commands.RegisterCommand;
using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Interfaces;
using EcomCourse.Application.Services;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Customers;
using Moq;

namespace EcomCourse.Application.Tests.Authentication;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<ICustomerStore> _customerStoreMock;
    private readonly CompensateAsync _compensateAsync;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _customerStoreMock = new Mock<ICustomerStore>();

        _compensateAsync = new CompensateAsync(
            _customerStoreMock.Object,
            _identityServiceMock.Object
        );

        _handler = new RegisterCommandHandler(
            _identityServiceMock.Object,
            _customerStoreMock.Object,
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

        _identityServiceMock
            .Setup(x => x.IsUserExist(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Identity.Register", result.Error.Code);

        _identityServiceMock.Verify(
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
        var error = new DomainError("Identity.CreationFailed", "Could not create user");

        _identityServiceMock
            .Setup(x => x.IsUserExist(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityServiceMock
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
        var userDto = new ApplicationUserDto { Id = dto.UserId, Email = dto.Email };

        _identityServiceMock
            .Setup(x => x.IsUserExist(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityServiceMock
            .Setup(x => x.CreateUserAsyncWithResult(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(userDto));

        _customerStoreMock
            .Setup(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityServiceMock
            .Setup(x => x.DeleteUserAsync(dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Customer.CreateFailed", result.Error.Code);

        _identityServiceMock.Verify(
            x => x.DeleteUserAsync(dto.UserId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenAllStepsSucceed_ReturnsSuccess()
    {
        var dto = CreateValidDto();
        var command = new RegisterCommand(dto);
        var userDto = new ApplicationUserDto { Id = dto.UserId, Email = dto.Email };

        _identityServiceMock
            .Setup(x => x.IsUserExist(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityServiceMock
            .Setup(x => x.CreateUserAsyncWithResult(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(userDto));

        _customerStoreMock
            .Setup(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _identityServiceMock
            .Setup(x =>
                x.SetCustomerIdAsync(dto.UserId, It.IsAny<Guid>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(Result.Success());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        _identityServiceMock.Verify(
            x => x.SetCustomerIdAsync(dto.UserId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _identityServiceMock.Verify(
            x => x.DeleteUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
