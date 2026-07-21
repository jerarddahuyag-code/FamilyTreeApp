using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTreeApp.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleFailure(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot handle failure for a successful result.");
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => NotFound(result.Error),
            ErrorType.Validation => BadRequest(result.Error),
            ErrorType.Conflict => Conflict(result.Error),
            ErrorType.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Error)
        };
    }
}
