using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Canvas.Commands;

public record AddTreeEdgeCommand : IRequest<Guid>
{
    public required Guid TreeId { get; init; }
    public required Guid SourceNodeId { get; init; }
    public required Guid TargetNodeId { get; init; }
}

public class AddTreeEdgeCommandHandler(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddTreeEdgeCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(AddTreeEdgeCommand command, CancellationToken cancellationToken = default)
    {
        TreeNode? sourceNode = await dbContext.TreeNodes
            .FirstOrDefaultAsync(n => n.Id == command.SourceNodeId && n.TreeId == command.TreeId, cancellationToken);

        TreeNode? targetNode = await dbContext.TreeNodes
            .FirstOrDefaultAsync(n => n.Id == command.TargetNodeId && n.TreeId == command.TreeId, cancellationToken);

        if (sourceNode is null || targetNode is null)
        {
            return Result.Failure<Guid>(DomainErrors.CanvasErrors.NodeNotInTree);
        }

        Result<TreeEdge> edgeResult = TreeEdge.Create(
            Guid.NewGuid(),
            command.TreeId,
            command.SourceNodeId,
            command.TargetNodeId);

        if (edgeResult.IsFailure)
        {
            return Result.Failure<Guid>(edgeResult.Error);
        }

        TreeEdge edge = edgeResult.Value;
        dbContext.TreeEdges.Add(edge);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(edge.Id);
    }
}
