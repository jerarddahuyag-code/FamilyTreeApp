using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Canvas.Enums;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Canvas.Commands;

public record UpdateTreeNodeCommand : IRequest<bool>
{
    public required Guid TreeId { get; init; }
    public required Guid NodeId { get; init; }
    public NodeType? NodeType { get; init; }
    public IReadOnlyList<Guid> FamilyMemberIds { get; init; } = [];
}

public class UpdateTreeNodeCommandHandler(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTreeNodeCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(UpdateTreeNodeCommand command, CancellationToken cancellationToken = default)
    {
        Guid[] distinctMemberIds = command.FamilyMemberIds.Distinct().ToArray();

        if (distinctMemberIds.Length > 0)
        {
            FamilyMember[] familyMembers = await dbContext.FamilyMembers
                .Where(m => m.TreeId == command.TreeId && distinctMemberIds.Contains(m.FamilyMemberId))
                .ToArrayAsync(cancellationToken);

            if (familyMembers.Length != distinctMemberIds.Length)
            {
                return Result.Failure<bool>(DomainErrors.CanvasErrors.MemberNotInTree);
            }
        }

        TreeNode? node = await dbContext.TreeNodes
            .Include(n => n.Members)
            .FirstOrDefaultAsync(n => n.Id == command.NodeId && n.TreeId == command.TreeId, cancellationToken);

        if (node == null)
        {
            return Result.Failure<bool>(DomainErrors.CanvasErrors.NodeNotFound);
        }

        if (command.NodeType.HasValue)
        {
            var typeResult = node.UpdateNodeType(command.NodeType.Value);
            if (typeResult.IsFailure)
            {
                return Result.Failure<bool>(typeResult.Error);
            }
        }
        
        var membersResult = node.UpdateMembers(distinctMemberIds);
        if (membersResult.IsFailure)
        {
            return Result.Failure<bool>(membersResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
