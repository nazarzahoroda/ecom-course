using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using FluentValidation;
using MediatR;

namespace EcomCourse.Application.Common.Behaviors
{
    public sealed class ValidationResultBehavior<TRequest, TResult>(IServiceProvider serviceProvider) : IPipelineBehavior<TRequest, TResult>
    where TRequest : notnull
    where TResult : notnull, IAsyncResult
    {
        public async Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)   
        {
            var validators = serviceProvider.GetServices<IValidator<TRequest>>();
            if (validators is null)
            {
                return await next();
            }

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorCode = validationResult.Errors.FirstOrDefault()?.ErrorCode ?? StatusCodes.Status400BadRequest.ToString();

                return (
                    int.TryParse(errorCode, out var code) ? code : StatusCodes.Status400BadRequest
                ) switch
                {
                    StatusCodes.Status403Forbidden => Results.Problem(
                        new ForbiddenResponse(validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? "Validation failed")  
                    ),
                    _ => Results.BadRequest(
                        new BadRequestResponse(validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? "Validation failed") 
                    ),
                };

                var failures = validationResults.SelectMany(result => result.Errors).Where(failure => failure is not null).ToList();
            
            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
            return await next();
        }
    }
}
