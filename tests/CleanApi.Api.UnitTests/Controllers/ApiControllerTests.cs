using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using CleanApi.Api.Controllers;
using CleanApi.Domain.Common;

namespace CleanApi.Api.UnitTests.Controllers;

public sealed class ApiControllerTests
{
    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Failure, StatusCodes.Status500InternalServerError)]
    public void Problem_MapsErrorTypeToStatusCode(ErrorType type, int expectedStatusCode)
    {
        var controller = CreateController();

        var result = controller.ToProblem(new Error("Test.Code", "Something happened.", type));

        Assert.Equal(expectedStatusCode, result.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(expectedStatusCode, problem.Status);
        Assert.Equal("Something happened.", problem.Detail);
        Assert.Equal("Test.Code", problem.Extensions["errorCode"]);
    }

    private static TestController CreateController()
    {
        var services = new ServiceCollection();
        services.AddControllers();
        var provider = services.BuildServiceProvider();

        return new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = provider },
            },
        };
    }

    /// <summary>Exposes the protected helper of the base controller for testing.</summary>
    private sealed class TestController : ApiController
    {
        public ObjectResult ToProblem(Error error) => Problem(error);
    }
}
