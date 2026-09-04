using EcomCourse.Application.Authentication.Commands.RefreshCommand;
using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;
using Moq;

namespace EcomCourse.Application.Tests.Authentication;

public class RefreshCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly RefreshCommandHandler _handler;

    public RefreshCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _handler = new RefreshCommandHandler(_identityServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenIsValid_ReturnsAuthResponse()
    {
        var refreshToken = "valid-refresh-token";
        var command = new RefreshCommand(refreshToken);

        var authResponse = new AuthResponse
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
        };

        _identityServiceMock
            .Setup(x => x.CheckRefreshToken(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(authResponse));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("new-access-token", result.Value.AccessToken);
        Assert.Equal("new-refresh-token", result.Value.RefreshToken);

        _identityServiceMock.Verify(
            x => x.CheckRefreshToken(refreshToken, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenIsInvalid_ReturnsFailure()
    {
        var refreshToken = "invalid-or-expired-token";
        var command = new RefreshCommand(refreshToken);
        var error = new DomainError(
            "Identity.InvalidRefreshToken",
            "Refresh token is invalid or expired."
        );

        _identityServiceMock
            .Setup(x => x.CheckRefreshToken(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AuthResponse>(error));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error.Code, result.Error.Code);
        Assert.Equal(error.Description, result.Error.Description);

        _identityServiceMock.Verify(
            x => x.CheckRefreshToken(refreshToken, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
