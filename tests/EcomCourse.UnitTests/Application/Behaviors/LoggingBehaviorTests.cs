using EcomCourse.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace EcomCourse.UnitTests.Application.Behaviors;

public class LoggingBehaviorTests
{
    public record TestCommand : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldLogProcessingAndCompleted()
    {
        // Arrange
        var loggerMock = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();

        loggerMock.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        var behavior = new LoggingBehavior<TestCommand, string>(loggerMock);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("Success");

        // Act
        var result = await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        Assert.Equal("Success", result);
        await next.Received(1).Invoke();

        var logCalls = loggerMock.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == "Log" && (LogLevel?)c.GetArguments()[0] == LogLevel.Information);

        Assert.Equal(2, logCalls.Count());
    }
}