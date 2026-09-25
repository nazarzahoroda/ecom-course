using EcomCourse.Application.Authentication.Commands.LogoutCommand;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;
using Moq;

namespace EcomCourse.Application.Tests.Authentication;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IIdentityProvider> _identityProviderMock;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _identityProviderMock = new Mock<IIdentityProvider>();
        _handler = new LogoutCommandHandler(_identityProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenIsValid_ReturnsSuccess()
    {
        var token = "valid-refresh-token";
        var command = new LogoutCommand(token);

        _identityProviderMock
            .Setup(x => x.RevokeRefreshToken(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        _identityProviderMock.Verify(
            x => x.RevokeRefreshToken(token, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenRevokeFails_ReturnsFailure()
    {
        var token = "invalid-token";
        var command = new LogoutCommand(token);
        var error = new DomainError(
                        "Auth.InvalidToken",
                        "Token is invalid",
                        ErrorType.Unauthorized);

        _identityProviderMock
            .Setup(x => x.RevokeRefreshToken(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error.Code, result.Error.Code);

        _identityProviderMock.Verify(
            x => x.RevokeRefreshToken(token, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
