using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Trees.Entities;

namespace FamilyTreeApp.Application.Trees.CQRS.Commands;

public record RemoveTreeAccessCommand : IRequest<bool>
{
    public required Guid TreeId { get; init; }
    public required Guid UserId { get; init; }
}

public class RemoveTreeAccessCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveTreeAccessCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(RemoveTreeAccessCommand command, CancellationToken cancellationToken = default)
    {
        TreeRbac? treeAccess = await context.TreeRbacs
            .FindAsync([command.TreeId, command.UserId], cancellationToken);
        if (treeAccess is null)
        {
            return Result.Failure<bool>(DomainErrors.TreeErrors.TreeAccessNotFound);
        }

        context.TreeRbacs.Remove(treeAccess);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
