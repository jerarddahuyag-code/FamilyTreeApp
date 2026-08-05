using FamilyTreeApp.Domain.Canvas.Enums;
using FamilyTreeApp.Domain.Canvas.ValueObjects;

namespace FamilyTreeApp.Application.Canvas.DTOs;

public record TreeNodeDto(
    Guid Id,
    NodeType Type,
    CanvasCoordinates Position,
    List<CanvasMemberDto> Members)
{
    public TreeNodeDto() : this(Guid.Empty, NodeType.Single, new CanvasCoordinates(0, 0), []) { }
}
