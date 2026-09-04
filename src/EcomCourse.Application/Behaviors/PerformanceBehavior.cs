using MediatR;
using Microsoft.Extensions.Logging;

namespace EcomCourse.Application.Behaviors;

public partial class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long SlowRequestThresholdMilliseconds = 500;

    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly TimeProvider _timeProvider;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var startTimestamp = _timeProvider.GetTimestamp();

        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            var elapsedTime = _timeProvider.GetElapsedTime(startTimestamp);

            if (elapsedTime.TotalMilliseconds > SlowRequestThresholdMilliseconds)
            {
                var requestName = typeof(TRequest).Name;
                LogLongRunningRequest(_logger, requestName, (long)elapsedTime.TotalMilliseconds);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Long running request: {RequestName} ({ElapsedMilliseconds} milliseconds)")]
    private static partial void LogLongRunningRequest(ILogger logger, string requestName, long elapsedMilliseconds);
}
