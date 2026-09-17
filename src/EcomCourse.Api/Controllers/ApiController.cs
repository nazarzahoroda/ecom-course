using EcomCourse.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Controllers
{
    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        protected IActionResult HandleFailure(Result result)
        {
            if (result.IsSuccess)
            {
                throw new InvalidOperationException("Cannot handle failure for a successful result.");
            }

            if (result is IValidationResult validationResult)
            {
                var dictionary = new Dictionary<string, string[]>();

                foreach (var group in validationResult.Errors.GroupBy(e => e.Code))
                {
                    dictionary.Add(group.Key, group.Select(e => e.Description).ToArray());
                }

                return ValidationProblem(new ValidationProblemDetails(dictionary)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = "One or more validation errors occurred."
                });
            }

            return BadRequest(new ProblemDetails
            {
                Title = result.Error.Code,
                Detail = result.Error.Description,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}