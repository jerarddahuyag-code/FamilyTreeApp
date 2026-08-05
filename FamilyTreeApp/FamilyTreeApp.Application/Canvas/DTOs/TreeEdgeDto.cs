namespace FamilyTreeApp.Application.Canvas.DTOs;

public record TreeEdgeDto(
    Guid Id,
    Guid SourceNodeId,
    Guid TargetNodeId)
{
    public TreeEdgeDto() : this(Guid.Empty, Guid.Empty, Guid.Empty) { }
}
