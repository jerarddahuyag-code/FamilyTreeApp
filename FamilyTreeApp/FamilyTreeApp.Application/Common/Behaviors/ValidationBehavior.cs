using FamilyTreeApp.Domain.Common;
using FluentValidation;
using FluentValidation.Results;

namespace FamilyTreeApp.Application.Common.Behaviors;

public sealed class ValidationPipelineBehavior<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> innerHandler,
    IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand, TResult>
{
    public async Task<Result<TResult>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!validators.Any())
            return await innerHandler.HandleAsync(command, cancellationToken);

        var context = new ValidationContext<TCommand>(command);
        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            return Result.Failure<TResult>(new Error("Error.Validation", errorMessage));
        }

        return await innerHandler.HandleAsync(command, cancellationToken);
    }
}
