using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyTreeApp.Application.Roster.CQRS.Commands;

public record ClaimFamilyMemberCommand : IRequest<bool>
{
    public Guid TreeId { get; init; }
    public Guid FamilyMemberId { get; init; }
    public Guid UserId { get; init; }
}

public class ClaimFamilyMemberCommandHandler(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ClaimFamilyMemberCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(ClaimFamilyMemberCommand command, CancellationToken cancellationToken = default)
    {
        var member = await dbContext.FamilyMembers.FirstOrDefaultAsync(m => m.FamilyMemberId == command.FamilyMemberId && m.TreeId == command.TreeId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<bool>(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
        }

        if (member.ClaimedByUserId.HasValue)
        {
            return Result.Failure<bool>(DomainErrors.FamilyMemberErrors.FamilyMemberClaimed);
        }

        member.UpdateClaimedBy(command.UserId);
        dbContext.FamilyMembers.Update(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}

