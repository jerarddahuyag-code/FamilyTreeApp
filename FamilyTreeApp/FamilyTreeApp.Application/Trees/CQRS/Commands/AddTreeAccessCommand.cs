using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Trees.Enums;

namespace FamilyTreeApp.Application.Trees.CQRS.Commands;

public record AddTreeAccessCommand : IRequest<Guid>
{
    public Guid TreeId { get; init; }
    public Guid UserId { get; init; }
    public required TreeRole AccessLevel { get; init; }
}

public class AddTreeAccessCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddTreeAccessCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(AddTreeAccessCommand command, CancellationToken cancellationToken = default)
    {
        Result<TreeRbac> result = TreeRbac.Create(Guid.NewGuid(), command.TreeId, command.UserId, command.AccessLevel);
        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        TreeRbac treeRbac = result.Value;
        await context.TreeRbacs.AddAsync(treeRbac, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(treeRbac.TreeRbacId);
    }
}
