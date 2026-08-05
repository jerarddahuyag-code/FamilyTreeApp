using FamilyTreeApp.Domain.Canvas.Enums;
using FamilyTreeApp.Domain.Canvas.ValueObjects;
using FamilyTreeApp.Domain.Common;

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
}
