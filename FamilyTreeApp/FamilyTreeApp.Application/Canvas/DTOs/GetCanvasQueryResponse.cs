namespace FamilyTreeApp.Application.Canvas.DTOs;

public record GetCanvasQueryResponse(List<TreeNodeDto> Nodes, List<TreeEdgeDto> Edges);
