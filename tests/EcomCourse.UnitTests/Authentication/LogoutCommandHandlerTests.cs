using EcomCourse.Application.Authentication.Commands.LogoutCommand;
using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Common;
using Moq;

namespace EcomCourse.Application.Tests.Authentication;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _handler = new LogoutCommandHandler(_identityServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenIsValid_ReturnsSuccess()
    {
        var token = "valid-refresh-token";
        var command = new LogoutCommand(token);

        _identityServiceMock
            .Setup(x => x.RevokeRefreshToken(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        _identityServiceMock.Verify(
            x => x.RevokeRefreshToken(token, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenRevokeFails_ReturnsFailure()
    {
        var token = "invalid-token";
        var command = new LogoutCommand(token);
        var error = new DomainError("Auth.InvalidToken", "Token is invalid");

        _identityServiceMock
            .Setup(x => x.RevokeRefreshToken(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error.Code, result.Error.Code);

        _identityServiceMock.Verify(
            x => x.RevokeRefreshToken(token, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
