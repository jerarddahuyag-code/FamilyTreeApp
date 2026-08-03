using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Interfaces;

namespace FamilyTreeApp.Application.Roster.CQRS.Commands;

public record RemoveRelationshipCommand : IRequest<bool>, ITransactionalCommand
{
    public required Guid TreeId { get; init; }
    public required Guid FamilyMemberRelationshipId { get; init; }
}

public class RemoveRelationshipCommandHandler(
    IFamilyMemberRelationshipRepository relationshipRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveRelationshipCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(RemoveRelationshipCommand command, CancellationToken cancellationToken = default)
    {
        FamilyMemberRelationship? relationship = await relationshipRepository.GetByIdAsync(command.FamilyMemberRelationshipId, cancellationToken);
        if (relationship is null || relationship.TreeId != command.TreeId)
        {
            return Result.Failure<bool>(DomainErrors.FamilyMemberRelationshipErrors.RelationshipNotFound);
        }

        relationshipRepository.Delete(relationship);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
