using FamilyTreeApp.Domain.Canvas.Enums;
using FamilyTreeApp.Domain.Canvas.ValueObjects;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;

namespace FamilyTreeApp.Domain.Canvas.Entities;

public class TreeNode
{
    public Guid Id { get; private set; }
    public Guid TreeId { get; private set; }
    public NodeType NodeType { get; private set; }
    public CanvasCoordinates Coordinates { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<TreeNodeMember> Members { get; private set; } = [];

    private TreeNode() { }

    private TreeNode(Guid id, Guid treeId, NodeType nodeType, CanvasCoordinates coordinates)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        TreeId = treeId;
        NodeType = nodeType;
        Coordinates = coordinates;
        CreatedAt = UpdatedAt = DateTime.UtcNow;
    }

    public static Result<TreeNode> Create(Guid id, Guid treeId, NodeType nodeType, CanvasCoordinates coordinates)
    {
        var node = new TreeNode(id, treeId, nodeType, coordinates);
        return Result.Success(node);
    }

    public Result UpdateCoordinates(double x, double y)
    {
        Coordinates = new CanvasCoordinates(x, y);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdateNodeType(NodeType newType)
    {
        int maxMembers = GetMaxMembersForNodeType(newType);

        if (Members.Count > maxMembers)
        {
            return Result.Failure(DomainErrors.CanvasErrors.NodeTypeLimitExceeded);
        }

        NodeType = newType;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result AddMember(Guid memberId)
    {
        if (Members.Any(m => m.FamilyMemberId == memberId))
        {
            return Result.Success();
        }

        int maxMembers = GetMaxMembersForNodeType(NodeType);

        if (Members.Count >= maxMembers)
        {
            return Result.Failure(DomainErrors.CanvasErrors.NodeTypeLimitExceeded);
        }

        Members.Add(new TreeNodeMember(Id, memberId));
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void RemoveMember(Guid memberId)
    {
        var member = Members.FirstOrDefault(m => m.FamilyMemberId == memberId);
        if (member != null)
        {
            Members.Remove(member);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public Result UpdateMembers(IEnumerable<Guid> memberIds)
    {
        var newMemberIds = memberIds.ToHashSet();

        int maxMembers = GetMaxMembersForNodeType(NodeType);

        if (newMemberIds.Count > maxMembers)
        {
            return Result.Failure(DomainErrors.CanvasErrors.NodeTypeLimitExceeded);
        }

        var existingMemberIds = Members.Select(m => m.FamilyMemberId).ToHashSet();

        var membersToRemove = Members.Where(m => !newMemberIds.Contains(m.FamilyMemberId)).ToList();
        foreach (var member in membersToRemove)
        {
            Members.Remove(member);
        }

        var memberIdsToAdd = newMemberIds.Except(existingMemberIds).ToList();
        foreach (var memberId in memberIdsToAdd)
        {
            Members.Add(new TreeNodeMember(Id, memberId));
        }

        if (membersToRemove.Count > 0 || memberIdsToAdd.Count > 0)
        {
            UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success();
    }

    private int GetMaxMembersForNodeType(NodeType nodeType)
    {
        return nodeType switch
        {
            NodeType.Single => 1,
            NodeType.Partner => 2,
            _ => int.MaxValue
        };
    }
}
