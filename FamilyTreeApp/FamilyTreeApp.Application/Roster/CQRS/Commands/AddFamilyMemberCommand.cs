using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.ValueObjects;

namespace FamilyTreeApp.Application.Roster.CQRS.Commands;

public record AddFamilyMemberCommand : IRequest<Guid>, ITransactionalCommand
{
    public required Guid TreeId { get; init; }
    public Guid? ClaimedByUserId { get; init; }
    public required ProfileInfo ProfileInfo { get; init; }
    public VisibilityStatus VisibilityStatus { get; init; } = VisibilityStatus.Hidden;
}

public class AddFamilyMemberCommandHandler(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddFamilyMemberCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(AddFamilyMemberCommand command, CancellationToken cancellationToken = default)
    {
        Result<FamilyMember> result = FamilyMember.Create(
            Guid.NewGuid(),
            command.TreeId,
            command.ClaimedByUserId,
            command.VisibilityStatus,
            command.ProfileInfo);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        FamilyMember member = result.Value;
        dbContext.FamilyMembers.Add(member);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(member.FamilyMemberId);
    }
}
