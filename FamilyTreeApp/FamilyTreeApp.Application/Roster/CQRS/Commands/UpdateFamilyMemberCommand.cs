using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Roster.CQRS.Commands;

public record UpdateFamilyMemberCommand : IRequest<bool>, ITransactionalCommand
{
    public required Guid FamilyMemberId { get; init; }
    public required Guid TreeId { get; init; }
    public required ProfileInfo ProfileInfo { get; init; }
    public Guid? ClaimedByUserId { get; init; }
}

public class UpdateFamilyMemberCommandHandler(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateFamilyMemberCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(UpdateFamilyMemberCommand command, CancellationToken cancellationToken = default)
    {
        FamilyMember? member = await dbContext.FamilyMembers.FirstOrDefaultAsync(m => m.FamilyMemberId == command.FamilyMemberId, cancellationToken);
        if (member is null || member.TreeId != command.TreeId)
        {
            return Result.Failure<bool>(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
        }

        // Create updated family member with same id & new info
        Result<FamilyMember> result = FamilyMember.Create(
            member.FamilyMemberId,
            member.TreeId,
            command.ClaimedByUserId ?? member.ClaimedByUserId,
            member.VisibilityStatus,
            command.ProfileInfo);

        if (result.IsFailure)
        {
            return Result.Failure<bool>(result.Error);
        }

        dbContext.FamilyMembers.Update(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
