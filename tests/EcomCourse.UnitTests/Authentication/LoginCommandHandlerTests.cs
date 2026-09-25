using EcomCourse.Application.Authentication.Commands.LoginCommand;
using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Abstractions.Authentication;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;
using Moq;

namespace EcomCourse.Application.Tests.Authentication;

public class LoginCommandHandlerTests
{
    private readonly Mock<IIdentityProvider> _identityProviderMock;
    private readonly Mock<ITokenIssuer> _tokenIssuerMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _identityProviderMock = new Mock<IIdentityProvider>();
        _tokenIssuerMock = new Mock<ITokenIssuer>();

        _handler = new LoginCommandHandler(_identityProviderMock.Object, _tokenIssuerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsFailure()
    {
        var dto = new LoginDto { Email = "user@example.com", Password = "Pass!12" };
        var command = new LoginCommand(dto);
        var error = new DomainError(
                        "Identity.UserNotFound",
                        "User not found",
                        ErrorType.NotFound);

        _identityProviderMock
            .Setup(x => x.GetUserAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ApplicationUserDto>(error));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error.Code, result.Error.Code);
        Assert.Equal(error.Description, result.Error.Description);

        _identityProviderMock.Verify(
            x => x.CheckPasswordSignInAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        _tokenIssuerMock.Verify(
            x => x.GenerateAccessToken(It.IsAny<UserTokenDetails>()),
            Times.Never
        );

        _tokenIssuerMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ReturnsFailure()
    {
        var dto = new LoginDto { Email = "user@example.com", Password = "Pass!12" };

        var user = new ApplicationUserDto { Id = Guid.NewGuid(), Email = dto.Email };

        var userResult = Result.Success(user);

        var error = new DomainError(
                        "Identity.InvalidCredentials",
                        "Invalid credentials.",
                        ErrorType.Unauthorized);

        _identityProviderMock
            .Setup(x => x.GetUserAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userResult);

        _identityProviderMock
            .Setup(x => x.CheckPasswordSignInAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

        var command = new LoginCommand(dto);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error.Code, result.Error.Code);
        Assert.Equal(error.Description, result.Error.Description);

        _identityProviderMock.Verify(
            x => x.GetRolesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        _tokenIssuerMock.Verify(
            x => x.GenerateAccessToken(It.IsAny<UserTokenDetails>()),
            Times.Never
        );

        _tokenIssuerMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenSaveFails_ReturnsRefreshTokenSaveError()
    {
        var dto = new LoginDto { Email = "user@example.com", Password = "Pass!12" };

        var user = new ApplicationUserDto { Id = Guid.NewGuid(), Email = dto.Email };

        var roles = new List<string> { "Customer" };

        var checkResult = Result.Success();

        var refreshToken = "refresh-token";

        var saveError = new DomainError(
                            "Identity.RefreshTokenSaveFailed",
                            "Failed to save refresh token.",
                            ErrorType.Conflict);

        _identityProviderMock
            .Setup(x => x.GetUserAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _identityProviderMock
            .Setup(x => x.CheckPasswordSignInAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkResult);

        _identityProviderMock
            .Setup(x => x.GetRolesAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        _tokenIssuerMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<UserTokenDetails>()))
            .Returns("access-token");

        _tokenIssuerMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);

        _identityProviderMock
            .Setup(x => x.SaveRefreshToken(refreshToken, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(saveError));

        var command = new LoginCommand(dto);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(saveError.Code, result.Error.Code);

        Assert.Equal(saveError.Description, result.Error.Description);
    }

    [Fact]
    public async Task Handle_WhenLoginIsSuccessful_ReturnsAuthResponse()
    {
        var dto = new LoginDto { Email = "user@example.com", Password = "Pass!12" };

        var user = new ApplicationUserDto
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            CustomerId = Guid.NewGuid(),
        };

        var roles = new List<string> { "Customer" };

        var accessToken = "access-token";
        var refreshToken = "refresh-token";

        _identityProviderMock
            .Setup(x => x.GetUserAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _identityProviderMock
            .Setup(x => x.CheckPasswordSignInAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _identityProviderMock
            .Setup(x => x.GetRolesAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        _tokenIssuerMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<UserTokenDetails>()))
            .Returns(accessToken);

        _tokenIssuerMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);

        _identityProviderMock
            .Setup(x => x.SaveRefreshToken(refreshToken, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var command = new LoginCommand(dto);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(accessToken, result.Value!.AccessToken);

        Assert.Equal(refreshToken, result.Value.RefreshToken);

        _tokenIssuerMock.Verify(
            x =>
                x.GenerateAccessToken(
                    It.Is<UserTokenDetails>(details =>
                        details.UserId == user.Id
                        && details.Email == user.Email
                        && details.CustomerId == user.CustomerId
                        && details.Roles.SequenceEqual(roles)
                    )
                ),
            Times.Once
        );

        _tokenIssuerMock.Verify(x => x.GenerateRefreshToken(), Times.Once);

        _identityProviderMock.Verify(
            x => x.SaveRefreshToken(refreshToken, user.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
