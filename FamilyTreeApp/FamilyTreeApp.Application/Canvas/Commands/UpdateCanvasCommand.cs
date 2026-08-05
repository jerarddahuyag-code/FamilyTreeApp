using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Canvas.Commands;

public record NodePositionUpdate(Guid NodeId, double X, double Y);

public record UpdateCanvasCommand : IRequest<bool>, ITransactionalCommand
{
    public required Guid TreeId { get; init; }
    public required IReadOnlyList<NodePositionUpdate> Updates { get; init; }
}

public class UpdateCanvasCommandHandler(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCanvasCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(UpdateCanvasCommand command, CancellationToken cancellationToken = default)
    {
        Guid[] targetNodeIds = [.. command.Updates.Select(u => u.NodeId).Distinct()];

        TreeNode[] nodes = await dbContext.TreeNodes
            .Where(n => n.TreeId == command.TreeId && targetNodeIds.Contains(n.Id))
            .ToArrayAsync(cancellationToken);

        if (nodes.Length != targetNodeIds.Length)
        {
            return Result.Failure<bool>(DomainErrors.CanvasErrors.NodeNotFound);
        }

        var lastUpdates = command.Updates
            .GroupBy(u => u.NodeId)
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (NodePositionUpdate? update in lastUpdates.Values)
        {
            TreeNode? node = nodes.FirstOrDefault(n => n.Id == update.NodeId);
            if (node is null)
            {
                return Result.Failure<bool>(DomainErrors.CanvasErrors.NodeNotFound);
            }

            Result updateResult = node.UpdateCoordinates(update.X, update.Y);
            if (updateResult.IsFailure)
            {
                return Result.Failure<bool>(updateResult.Error);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
