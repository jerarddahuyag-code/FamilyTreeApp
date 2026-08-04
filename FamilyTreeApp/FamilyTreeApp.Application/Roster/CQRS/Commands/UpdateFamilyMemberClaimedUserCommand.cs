using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Roster.CQRS.Commands;

public record UpdateFamilyMemberClaimedUserCommand : IRequest<bool>
{
    public required Guid FamilyMemberId { get; init; }
    public required Guid TreeId { get; init; }
    public required Guid? ClaimedByUserId { get; init; }
}

public class UpdateFamilyMemberClaimedUserCommandHandler(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateFamilyMemberClaimedUserCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(UpdateFamilyMemberClaimedUserCommand command, CancellationToken cancellationToken = default)
    {
        FamilyMember? member = await dbContext.FamilyMembers.FirstOrDefaultAsync(m =>
            m.FamilyMemberId == command.FamilyMemberId
            && m.TreeId == command.TreeId,
            cancellationToken);
        if (member is null)
        {
            return Result.Failure<bool>(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
        }

        member.UpdateClaimedBy(command.ClaimedByUserId);
        dbContext.FamilyMembers.Update(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
