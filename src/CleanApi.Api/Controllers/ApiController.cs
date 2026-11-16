using Microsoft.AspNetCore.Mvc;
using CleanApi.Domain.Common;

namespace CleanApi.Api.Controllers;

/// <summary>
/// Base class for all API controllers. Converts a failed <see cref="Result"/> into a problem details response.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiController : ControllerBase
{
    protected ObjectResult Problem(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            HttpContext, statusCode, detail: error.Description);
        problemDetails.Extensions["errorCode"] = error.Code;

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
