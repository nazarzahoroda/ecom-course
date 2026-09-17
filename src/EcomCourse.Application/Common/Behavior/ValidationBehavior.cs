using System.Reflection;
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
            if (!_validators.Any())
            {
                return await next(cancellationToken);
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

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

                var validationResultType = typeof(ValidationResult<>).MakeGenericType(valueType);

                var withErrorsMethod = validationResultType.GetMethod(
                    nameof(ValidationResult.WithErrors),
                    BindingFlags.Public | BindingFlags.Static);

                var result = withErrorsMethod!.Invoke(null, new object[] { errors })!;
                return (TResult)result;
            }

            return (TResult)(object)ValidationResult.WithErrors(errors);
        }
    }
}
