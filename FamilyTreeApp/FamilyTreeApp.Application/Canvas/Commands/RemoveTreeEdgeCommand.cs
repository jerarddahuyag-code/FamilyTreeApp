using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Canvas.Commands;

public record RemoveTreeEdgeCommand : IRequest<bool>
{
    public required Guid TreeId { get; init; }
    public required Guid EdgeId { get; init; }
}

public class RemoveTreeEdgeCommandHandler(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveTreeEdgeCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(RemoveTreeEdgeCommand command, CancellationToken cancellationToken = default)
    {
        TreeEdge? edge = await dbContext.TreeEdges
            .FirstOrDefaultAsync(e => e.Id == command.EdgeId && e.TreeId == command.TreeId, cancellationToken);

        if (edge is null)
        {
            return Result.Failure<bool>(DomainErrors.CanvasErrors.EdgeNotFound);
        }

        dbContext.TreeEdges.Remove(edge);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
