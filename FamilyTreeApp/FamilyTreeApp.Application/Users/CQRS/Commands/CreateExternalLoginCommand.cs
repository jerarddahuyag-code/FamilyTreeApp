using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Users.Entities;

namespace FamilyTreeApp.Application.Users.CQRS.Commands;

public record CreateExternalLoginCommand : IRequest<Guid>
{
    public required Guid ExternalLoginId { get; init; }
    public required Guid UserId { get; init; }
    public required string Provider { get; init; }
    public required string ProviderKey { get; init; }
}

public class CreateExternalLoginCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateExternalLoginCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateExternalLoginCommand command, CancellationToken cancellationToken = default)
    {
        var createExternalResult = ExternalLogin.Create(command.ExternalLoginId, command.UserId, command.Provider, command.ProviderKey);
        if (createExternalResult.IsFailure)
        {
            return Result.Failure<Guid>(createExternalResult.Error);
        }

        await context.ExternalLogins.AddAsync(createExternalResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(createExternalResult.Value.ExternalLoginId);
    }
}
