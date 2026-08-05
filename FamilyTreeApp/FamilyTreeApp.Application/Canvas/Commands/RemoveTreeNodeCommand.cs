using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Canvas.Commands;

public record RemoveTreeNodeCommand : IRequest<bool>, ITransactionalCommand
{
    public required Guid TreeId { get; init; }
    public required Guid NodeId { get; init; }
}

public class RemoveTreeNodeCommandHandler(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveTreeNodeCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(RemoveTreeNodeCommand command, CancellationToken cancellationToken = default)
    {
        TreeNode? node = await dbContext.TreeNodes
            .FirstOrDefaultAsync(n => n.Id == command.NodeId && n.TreeId == command.TreeId, cancellationToken);

        if (node is null)
        {
            return Result.Failure<bool>(DomainErrors.CanvasErrors.NodeNotFound);
        }

        dbContext.TreeNodes.Remove(node);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
