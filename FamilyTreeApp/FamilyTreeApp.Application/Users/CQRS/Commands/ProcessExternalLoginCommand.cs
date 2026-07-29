using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FamilyTreeApp.Application.Users.CQRS.Commands;

public record ProcessExternalLoginCommand : IRequest<bool>
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
    IAuthService authService,
    ICommandHandler<CreateUserCommand, Guid> createUserHandler,
    ICommandHandler<CreateExternalLoginCommand, Guid> createExternalLoginHandler)
    : ICommandHandler<ProcessExternalLoginCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(ProcessExternalLoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.ProviderKey) || string.IsNullOrWhiteSpace(command.Email))
        {
            return Result.Failure<bool>(Domain.Common.Errors.DomainErrors.ExternalLoginErrors.InvalidProviderKey);
        }

        var existingLogin = await context.ExternalLogins.FirstOrDefaultAsync(x => x.Provider == command.Provider && x.ProviderKey == command.ProviderKey, cancellationToken);
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == command.Email && u.DeletedAt == null, cancellationToken);
        Guid userId = existingUser?.UserId ?? Guid.Empty;
        
        if (existingUser == null)
        {
            var createResult = await createUserHandler.HandleAsync(new CreateUserCommand
                {
                    Email = command.Email,
                    FirstName = command.GivenName ?? (command.Name?.Split(' ').FirstOrDefault()),
                    LastName = command.FamilyName ?? (command.Name?.Split(' ').Skip(1).FirstOrDefault()),
                    AvatarUrl = command.Picture,
                    IsPublic = true
                }, cancellationToken);

            if (createResult.IsFailure)
            {
                return Result.Failure<bool>(createResult.Error);
            }

            userId = createResult.Value;
        }

        if (existingLogin == null)
        {
            var createExternalResult = await createExternalLoginHandler.HandleAsync(new CreateExternalLoginCommand
            {
                ExternalLoginId = Guid.NewGuid(),
                UserId = userId,
                Provider = command.Provider,
                ProviderKey = command.ProviderKey
            }, cancellationToken);

            if (createExternalResult.IsFailure)
            {
                return Result.Failure<bool>(createExternalResult.Error);
            }
        }

        await authService.SignInAsync(userId, command.Email, command.Name);

        return Result.Success(true);
    }
}
