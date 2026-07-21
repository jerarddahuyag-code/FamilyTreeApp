using FamilyTreeApp.Domain.Common.Errors;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace FamilyTreeApp.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred.");

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var error = Error.Failure;
        var response = JsonSerializer.Serialize(new { code = error.Code, message = error.Message, type = error.Type.ToString() });
        await httpContext.Response.WriteAsync(response, cancellationToken);

        return true;
    }
}
