using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.Roster.Interfaces;

namespace FamilyTreeApp.Application.Roster.CQRS.Commands;

public record RequestVisibilityCommand : IRequest<bool>, ITransactionalCommand
{
    public required Guid FamilyMemberId { get; init; }
    public required Guid TreeId { get; init; }
    public required VisibilityStatus TargetVisibilityStatus { get; init; }
}

public class RequestVisibilityCommandHandler(
    IFamilyMemberRepository familyMemberRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RequestVisibilityCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(RequestVisibilityCommand command, CancellationToken cancellationToken = default)
    {
        FamilyMember? member = await familyMemberRepository.GetByIdAsync(command.FamilyMemberId, cancellationToken);
        if (member is null || member.TreeId != command.TreeId)
        {
            return Result.Failure<bool>(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
        }

        Result result = member.TransitionToVisbility(command.TargetVisibilityStatus);
        if (result.IsFailure)
        {
            return Result.Failure<bool>(result.Error);
        }

        familyMemberRepository.Update(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
