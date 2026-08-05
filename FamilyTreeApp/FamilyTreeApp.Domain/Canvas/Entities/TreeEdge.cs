using FamilyTreeApp.Domain.Common;

namespace FamilyTreeApp.Domain.Canvas.Entities;

public class TreeEdge
{
    public Guid Id { get; private set; }
    public Guid TreeId { get; private set; }
    public Guid SourceNodeId { get; private set; }
    public Guid TargetNodeId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TreeEdge() { }

    private TreeEdge(Guid id, Guid treeId, Guid sourceNodeId, Guid targetNodeId)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        TreeId = treeId;
        SourceNodeId = sourceNodeId;
        TargetNodeId = targetNodeId;
        CreatedAt = UpdatedAt = DateTime.UtcNow;
    }

    public static Result<TreeEdge> Create(Guid id, Guid treeId, Guid sourceNodeId, Guid targetNodeId)
    {
        var edge = new TreeEdge(id, treeId, sourceNodeId, targetNodeId);
        return Result.Success(edge);
    }
}
