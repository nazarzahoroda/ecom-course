using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EcomCourse.Domain.Common;
using FluentValidation;
using MediatR;

namespace EcomCourse.Application.Common.Behavior
{
    public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) =>
            _validators = validators;

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (_validators is ICollection<IValidator<TRequest>> validatorsCollection)
            {
                if (validatorsCollection.Count == 0)
                {
                    return await next(cancellationToken);
                }
            }
            else
            {
                using var enumerator = _validators.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return await next(cancellationToken);
                }
            }

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(request, cancellationToken)));

            DomainError[] errors = validationResults
                .SelectMany(validationResult => validationResult.Errors)
                .Where(validationFailure => validationFailure is not null)
                .Select(failure => new DomainError(
                    failure.PropertyName,
                    failure.ErrorMessage))
                .Distinct()
                .ToArray();

            if (errors.Length > 0)
            {
                return CreateValidationResult<TResponse>(errors);
            }

            return await next(cancellationToken);
        }

        private static TResult CreateValidationResult<TResult>(DomainError[] errors)
            where TResult : Result
        {
            if (typeof(TResult) == typeof(Result))
            {
                return (ValidationResult.WithErrors(errors) as TResult)!;
            }

            if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var valueType = typeof(TResult).GenericTypeArguments[0];
                var combinedMessage = string.Join("; ", errors.Select(e => e.Description));

                var combinedError = new DomainError("Validation", combinedMessage);

                var failureMethod = typeof(Result)
                    .GetMethod(nameof(Result.Failure), BindingFlags.Public | BindingFlags.Static)?
                    .MakeGenericMethod(valueType);

                var result = failureMethod!.Invoke(null, new object[] { combinedError })!;
                return (TResult)result;
            }

            return (TResult)(object)ValidationResult.WithErrors(errors);
        }
    }
}
