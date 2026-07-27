using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Users.CQRS.Commands;

public record ProcessExternalLoginCommand : IRequest<Guid>
{
    public required string Provider { get; init; }
    public required string ProviderKey { get; init; }
    public required string Email { get; init; }
    public string? Name { get; init; }
    public string? GivenName { get; init; }
    public string? FamilyName { get; init; }
    public string? Picture { get; init; }
}

public class ProcessExternalLoginCommandHandler(
    IApplicationDbContext context,
    ICommandHandler<CreateUserCommand, Guid> createUserHandler,
    ICommandHandler<CreateExternalLoginCommand, Guid> createExternalLoginHandler)
    : ICommandHandler<ProcessExternalLoginCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(ProcessExternalLoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.ProviderKey) || string.IsNullOrWhiteSpace(command.Email))
        {
            return Result.Failure<Guid>(Domain.Common.Errors.DomainErrors.ExternalLoginErrors.InvalidProviderKey);
        }

        var existingLogin = await context.ExternalLogins
            .FirstOrDefaultAsync(x => x.Provider == command.Provider && x.ProviderKey == command.ProviderKey, cancellationToken);

        Guid userId;
        if (existingLogin != null)
        {
            return Result.Success(existingLogin.UserId);
        }

        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        if (existingUser != null)
        {
            userId = existingUser.UserId;
        }
        else
        {
            var createUserCmd = new CreateUserCommand
            {
                Email = command.Email,
                FirstName = command.GivenName ?? (command.Name?.Split(' ').FirstOrDefault()),
                LastName = command.FamilyName ?? (command.Name?.Split(' ').Skip(1).FirstOrDefault()),
                AvatarUrl = command.Picture,
                IsPublic = true
            };

            var createResult = await createUserHandler.HandleAsync(createUserCmd, cancellationToken);
            if (createResult.IsFailure)
            {
                return Result.Failure<Guid>(createResult.Error);
            }

            userId = createResult.Value;
        }

        var externalCmd = new CreateExternalLoginCommand
        {
            ExternalLoginId = Guid.NewGuid(),
            UserId = userId,
            Provider = command.Provider,
            ProviderKey = command.ProviderKey
        };

        var createExternalResult = await createExternalLoginHandler.HandleAsync(externalCmd, cancellationToken);
        if (createExternalResult.IsFailure)
        {
            return Result.Failure<Guid>(createExternalResult.Error);
        }

        return Result.Success(userId);
    }
}
