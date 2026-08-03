using System.Diagnostics;
using FamilyTreeApp.Domain.Common;
using Microsoft.Extensions.Logging;

namespace FamilyTreeApp.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ICommandHandler<TRequest, TResponse> innerHandler,
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : ICommandHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<Result<TResponse>> HandleAsync(
        TRequest command,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        Result<TResponse> result = await innerHandler.HandleAsync(command, cancellationToken);
        sw.Stop();

        string commandType = typeof(TRequest).Name;

        if (result.IsSuccess)
        {
            logger.LogInformation(
                "Command {CommandType} succeeded in {ElapsedMs}ms",
                commandType,
                sw.ElapsedMilliseconds);
        }
        else
        {
            logger.LogWarning(
                "Command {CommandType} failed in {ElapsedMs}ms: [{ErrorCode}] {ErrorDescription}",
                commandType,
                sw.ElapsedMilliseconds,
                result.Error.Code,
                result.Error.Message);
        }

        return result;
    }
}
