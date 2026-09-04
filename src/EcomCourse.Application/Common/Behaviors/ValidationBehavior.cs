using EcomCourse.Domain.Common;
using FluentValidation;
using MediatR;

namespace EcomCourse.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(validator =>
                validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        var error = new DomainError(
            "Validation.Error",
            string.Join("; ", failures.Select(failure => failure.ErrorMessage)));

        return CreateFailureResponse(error);
    }

    private static TResponse CreateFailureResponse(DomainError error)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(TResponse).GetGenericArguments()[0];

            var failureMethod = typeof(Result)
                .GetMethods()
                .Single(method =>
                    method.Name == nameof(Result.Failure) &&
                    method.IsGenericMethod);

            var genericFailureMethod = failureMethod.MakeGenericMethod(valueType);

            return (TResponse)genericFailureMethod.Invoke(
                null,
                new object[] { error })!;
        }

        throw new InvalidOperationException(
            $"ValidationBehavior supports only Result responses. " +
            $"Actual response type: {typeof(TResponse).Name}");
    }
}
