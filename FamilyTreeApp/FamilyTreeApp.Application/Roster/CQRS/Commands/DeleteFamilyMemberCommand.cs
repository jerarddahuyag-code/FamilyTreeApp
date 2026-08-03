using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Interfaces;

namespace FamilyTreeApp.Application.Roster.CQRS.Commands;

public record DeleteFamilyMemberCommand : IRequest<bool>, ITransactionalCommand
{
    public required Guid TreeId { get; init; }
    public required Guid FamilyMemberId { get; init; }
}

public class DeleteFamilyMemberCommandHandler(
    IFamilyMemberRepository familyMemberRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteFamilyMemberCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(DeleteFamilyMemberCommand command, CancellationToken cancellationToken = default)
    {
        FamilyMember? member = await familyMemberRepository.GetByIdAsync(command.FamilyMemberId, cancellationToken);
        if (member is null || member.TreeId != command.TreeId)
        {
            return Result.Failure<bool>(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
        }

        familyMemberRepository.Delete(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
