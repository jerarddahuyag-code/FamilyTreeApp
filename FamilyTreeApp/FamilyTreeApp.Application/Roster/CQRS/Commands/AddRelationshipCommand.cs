using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Roster.CQRS.Commands;

public record AddRelationshipCommand : IRequest<Guid>, ITransactionalCommand
{
    public required Guid TreeId { get; init; }
    public required Guid BaseFamilyMemberId { get; init; }
    public required Guid RelatedFamilyMemberId { get; init; }
    public required RelationshipType RelationshipType { get; init; }
}

public class AddRelationshipCommandHandler(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddRelationshipCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(AddRelationshipCommand command, CancellationToken cancellationToken = default)
    {
        FamilyMember? baseMember = await dbContext.FamilyMembers.FirstOrDefaultAsync(m => m.FamilyMemberId == command.BaseFamilyMemberId, cancellationToken);
        FamilyMember? relatedMember = await dbContext.FamilyMembers.FirstOrDefaultAsync(m => m.FamilyMemberId == command.RelatedFamilyMemberId, cancellationToken);

        if (baseMember is null || relatedMember is null)
        {
            return Result.Failure<Guid>(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
        }

        if (baseMember.TreeId != command.TreeId || relatedMember.TreeId != command.TreeId)
        {
            return Result.Failure<Guid>(DomainErrors.FamilyMemberRelationshipErrors.MemberTreeMismatch);
        }

        Result<FamilyMemberRelationship> result = FamilyMemberRelationship.Create(
            Guid.NewGuid(),
            command.TreeId,
            command.BaseFamilyMemberId,
            command.RelatedFamilyMemberId,
            command.RelationshipType);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        FamilyMemberRelationship relationship = result.Value;
        dbContext.FamilyMemberRelationships.Add(relationship);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(relationship.FamilyMemberRelationshipId);
    }
}
