using FamilyTreeApp.Domain.Common.Errors;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace FamilyTreeApp.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred.");
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";
        Error error = Error.Failure;
        object responseObj;
        // 2. Check if we are in Development mode
        if (env.IsDevelopment())
        {
            // Return detailed exception information
            responseObj = new
            {
                code = error.Code,
                message = exception.Message,
                type = error.Type.ToString(),
                details = exception.StackTrace
            };
        }
        else
        {
            // Return generic error for Production to stay secure
            responseObj = new
            {
                code = error.Code,
                message = error.Message,
                type = error.Type.ToString()
            };
        }

        var response = JsonSerializer.Serialize(responseObj);
        await httpContext.Response.WriteAsync(response, cancellationToken);
        return true;
    }
}
