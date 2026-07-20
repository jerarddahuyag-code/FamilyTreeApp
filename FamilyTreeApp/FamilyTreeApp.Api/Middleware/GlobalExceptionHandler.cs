using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

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

        var response = JsonSerializer.Serialize(new { error = "An unexpected error occurred." });
        await httpContext.Response.WriteAsync(response, cancellationToken);

        return true;
    }
}
