using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Canvas.Enums;
using FamilyTreeApp.Domain.Canvas.ValueObjects;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Canvas.Commands;

public record AddTreeNodeCommand : IRequest<Guid>
{
    public required Guid TreeId { get; init; }
    public required NodeType NodeType { get; init; }
    public required double X { get; init; }
    public required double Y { get; init; }
    public IReadOnlyList<Guid> FamilyMemberIds { get; init; } = [];
}

public class AddTreeNodeCommandHandler(
    IApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddTreeNodeCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(AddTreeNodeCommand command, CancellationToken cancellationToken = default)
    {
        Guid[] distinctMemberIds = command.FamilyMemberIds.Distinct().ToArray();

        if (distinctMemberIds.Length > 0)
        {
            FamilyMember[] familyMembers = await dbContext.FamilyMembers
                .Where(m => m.TreeId == command.TreeId && distinctMemberIds.Contains(m.FamilyMemberId))
                .ToArrayAsync(cancellationToken);

            if (familyMembers.Length != distinctMemberIds.Length)
            {
                return Result.Failure<Guid>(DomainErrors.CanvasErrors.MemberNotInTree);
            }
        }

        var coordinates = new CanvasCoordinates(command.X, command.Y);
        Result<TreeNode> nodeResult = TreeNode.Create(Guid.NewGuid(), command.TreeId, command.NodeType, coordinates);

        if (nodeResult.IsFailure)
        {
            return Result.Failure<Guid>(nodeResult.Error);
        }

        TreeNode node = nodeResult.Value;

        foreach (Guid memberId in distinctMemberIds)
        {
            var addResult = node.AddMember(memberId);
            if (addResult.IsFailure)
            {
                return Result.Failure<Guid>(addResult.Error);
            }
        }

        dbContext.TreeNodes.Add(node);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(node.Id);
    }
}
