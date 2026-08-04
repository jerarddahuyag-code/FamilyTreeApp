using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Roster.CQRS.Commands;

public record UnclaimFamilyMemberCommand : IRequest<bool>
{
    public Guid TreeId { get; init; }
    public Guid FamilyMemberId { get; init; }
    public Guid UserId { get; init; }
}

public class UnclaimFamilyMemberCommandHandler(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UnclaimFamilyMemberCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(UnclaimFamilyMemberCommand command, CancellationToken cancellationToken = default)
    {
        FamilyMember? member = await dbContext.FamilyMembers.FirstOrDefaultAsync(m => m.FamilyMemberId == command.FamilyMemberId && m.TreeId == command.TreeId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<bool>(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
        }

        if (member.ClaimedByUserId != command.UserId)
        {
            return Result.Failure<bool>(DomainErrors.FamilyMemberErrors.UserNotAuthorizedToUnclaim);
        }

        member.UpdateClaimedBy(null);
        dbContext.FamilyMembers.Update(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
