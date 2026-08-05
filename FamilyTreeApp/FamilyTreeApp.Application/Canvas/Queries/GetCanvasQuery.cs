using FamilyTreeApp.Application.Canvas.DTOs;
using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Trees.Services;
using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Canvas.Services;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.ValueObjects;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.Trees.Enums;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Canvas.Queries;

public record GetCanvasQuery : IRequest<GetCanvasQueryResponse>
{
    public required Guid TreeId { get; init; }
    public required Guid RequestingUserId { get; init; }
}

public class GetCanvasQueryHandler(
    IApplicationDbContext dbContext,
    IVisibilityService visibilityService,
    ITreeRoleService treeRoleService)
    : IQueryHandler<GetCanvasQuery, GetCanvasQueryResponse>
{
    public async Task<Result<GetCanvasQueryResponse>> HandleAsync(GetCanvasQuery query, CancellationToken cancellationToken = default)
    {
        TreeRole? requesterRole = await treeRoleService.GetUserRoleAsync(query.TreeId, query.RequestingUserId, cancellationToken);

        List<TreeNode> nodes = await dbContext.TreeNodes
            .AsNoTracking()
            .Include(n => n.Members)
                .ThenInclude(m => m.FamilyMember)
                    .ThenInclude(f => f.ClaimedByUser)
            .Where(n => n.TreeId == query.TreeId)
            .ToListAsync(cancellationToken);

        List<TreeEdge> edges = await dbContext.TreeEdges
            .AsNoTracking()
            .Where(e => e.TreeId == query.TreeId)
            .ToListAsync(cancellationToken);

        Dictionary<Guid, CanvasMemberVisibility> visibilityMap = visibilityService.ResolveForCanvas(nodes, requesterRole);

        var nodeDtos = nodes.Select(node =>
        {
            var memberDtos = node.Members
                .Select(m =>
                {
                    if (visibilityMap.TryGetValue(m.FamilyMemberId, out CanvasMemberVisibility? vis))
                    {
                        return new CanvasMemberDto(
                            vis.FamilyMemberId,
                            vis.ProfileInfo,
                            vis.IsMasked,
                            vis.VisibilityStatus);
                    }

                    return new CanvasMemberDto(
                        m.FamilyMemberId,
                        m.FamilyMember?.ProfileInfo ?? ProfileInfo.CreateAnonymous(),
                        true,
                        m.FamilyMember?.VisibilityStatus ?? VisibilityStatus.Hidden);
                })
                .ToList();

            return new TreeNodeDto(
                node.Id,
                node.NodeType,
                node.Coordinates,
                memberDtos);
        }).ToList();

        var edgeDtos = edges
            .Select(e => new TreeEdgeDto(e.Id, e.SourceNodeId, e.TargetNodeId))
            .ToList();

        var canvasDto = new GetCanvasQueryResponse(nodeDtos, edgeDtos);
        return Result.Success(canvasDto);
    }
}
