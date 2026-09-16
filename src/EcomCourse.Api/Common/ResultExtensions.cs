using EcomCourse.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Common;

public static class ResultExtensions
{
    public static IActionResult ToProblemDetails(this Result result)
    {
        var statusCode = result.Error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };

        var problemDetails = new ProblemDetails
        {
            Title = result.Error.Code,
            Detail = result.Error.Description,
            Status = statusCode
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }
}
