using EcomCourse.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace EcomCourse.UnitTests.Application.Behaviors;

public class PerformanceBehaviorTests
{
    public record TestCommand : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldNotLogWarning_WhenRequestIsFast()
    {
        // Arrange
        var loggerMock = Substitute.For<ILogger<PerformanceBehavior<TestCommand, string>>>();
        loggerMock.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        var timeProvider = new FakeTimeProvider();
        var behavior = new PerformanceBehavior<TestCommand, string>(loggerMock, timeProvider);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("Success");

        // Act
        var result = await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        Assert.Equal("Success", result);

        var warningLogs = loggerMock.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == "Log" && (LogLevel?)c.GetArguments()[0] == LogLevel.Warning);

        Assert.Empty(warningLogs);
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_WhenRequestTakesLongerThan500ms()
    {
        // Arrange
        var loggerMock = Substitute.For<ILogger<PerformanceBehavior<TestCommand, string>>>();
        loggerMock.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        var timeProvider = new FakeTimeProvider();
        var behavior = new PerformanceBehavior<TestCommand, string>(loggerMock, timeProvider);

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns(async _ =>
        {
            timeProvider.Advance(TimeSpan.FromMilliseconds(510));
            return "Success";
        });

        // Act
        var result = await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        Assert.Equal("Success", result);

        var warningLogs = loggerMock.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == "Log" && (LogLevel?)c.GetArguments()[0] == LogLevel.Warning);

        Assert.Single(warningLogs);
    }
}