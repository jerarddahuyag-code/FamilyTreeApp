using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FluentValidation;
using FluentValidation.Results;

namespace FamilyTreeApp.Application.Common.Behaviors;

public sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    ICommandHandler<TRequest, TResponse> innerHandler,
    IEnumerable<IValidator<TRequest>> validators) 
    : ICommandHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<Result<TResponse>> HandleAsync(
        TRequest command,
        CancellationToken cancellationToken = default)
    {
        if (!validators.Any())
        {
            return await innerHandler.HandleAsync(command, cancellationToken);
        }

        var context = new ValidationContext<TRequest>(command);
        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            return Result.Failure<TResponse>(Error.Validation);
        }

        return await innerHandler.HandleAsync(command, cancellationToken);
    }
}
