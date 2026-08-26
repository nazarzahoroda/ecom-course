using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {

        public async ValueTask<bool> TryHandleAsync(
         HttpContext httpContext,
         Exception exception,
         CancellationToken cancellationToken)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = exception.Message

            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);


            return true;
        }

    }
}
