using FamilyTreeApp.Application.Canvas.DTOs;
using FamilyTreeApp.Domain.Canvas.Enums;
using FamilyTreeApp.Domain.Canvas.ValueObjects;
using FamilyTreeApp.Domain.Common.ValueObjects;
using FamilyTreeApp.Domain.Roster.Enums;

namespace FamilyTreeApp.Tests.Unit.Application.Canvas;

public class CanvasDtoTests
{
    [Fact(DisplayName = "CanvasDto parameterless constructor initializes empty collections")]
    public void CanvasDto_DefaultConstructor_InitializesEmptyLists()
    {
        // Act
        var dto = new GetCanvasQueryResponse();

        // Assert
        Assert.NotNull(dto.Nodes);
        Assert.Empty(dto.Nodes);
        Assert.NotNull(dto.Edges);
        Assert.Empty(dto.Edges);
    }

    [Fact(DisplayName = "CanvasDto parameterized constructor maps nodes and edges correctly")]
    public void CanvasDto_ParameterizedConstructor_SetsProperties()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var profile = new ProfileInfo { FirstName = "John", LastName = "Doe" };
        var memberDto = new CanvasMemberDto(memberId, profile, false, VisibilityStatus.Visible);

        var nodeId = Guid.NewGuid();
        var position = new CanvasCoordinates(10.5, 20.5);
        var nodeDto = new TreeNodeDto(nodeId, NodeType.Single, position, [memberDto]);

        var edgeId = Guid.NewGuid();
        var sourceNodeId = Guid.NewGuid();
        var targetNodeId = Guid.NewGuid();
        var edgeDto = new TreeEdgeDto(edgeId, sourceNodeId, targetNodeId);

        // Act
        var canvasDto = new GetCanvasQueryResponse([nodeDto], [edgeDto]);

        // Assert
        Assert.Single(canvasDto.Nodes);
        Assert.Equal(nodeId, canvasDto.Nodes[0].Id);
        Assert.Equal(NodeType.Single, canvasDto.Nodes[0].Type);
        Assert.Equal(position, canvasDto.Nodes[0].Position);
        Assert.Single(canvasDto.Nodes[0].Members);
        Assert.Equal(memberId, canvasDto.Nodes[0].Members[0].Id);
        Assert.Equal("John", canvasDto.Nodes[0].Members[0].ProfileInfo.FirstName);
        Assert.False(canvasDto.Nodes[0].Members[0].IsMasked);
        Assert.Equal(VisibilityStatus.Visible, canvasDto.Nodes[0].Members[0].VisibilityStatus);

        Assert.Single(canvasDto.Edges);
        Assert.Equal(edgeId, canvasDto.Edges[0].Id);
        Assert.Equal(sourceNodeId, canvasDto.Edges[0].SourceNodeId);
        Assert.Equal(targetNodeId, canvasDto.Edges[0].TargetNodeId);
    }

    [Fact(DisplayName = "TreeEdgeDto default constructor sets empty Guids")]
    public void TreeEdgeDto_DefaultConstructor_SetsEmptyGuids()
    {
        // Act
        var dto = new TreeEdgeDto();

        // Assert
        Assert.Equal(Guid.Empty, dto.Id);
        Assert.Equal(Guid.Empty, dto.SourceNodeId);
        Assert.Equal(Guid.Empty, dto.TargetNodeId);
    }

    [Fact(DisplayName = "TreeNodeDto default constructor sets default values")]
    public void TreeNodeDto_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var dto = new TreeNodeDto();

        // Assert
        Assert.Equal(Guid.Empty, dto.Id);
        Assert.Equal(NodeType.Single, dto.Type);
        Assert.Equal(new CanvasCoordinates(0, 0), dto.Position);
        Assert.Empty(dto.Members);
    }

    [Fact(DisplayName = "CanvasMemberDto default constructor sets default values")]
    public void CanvasMemberDto_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var dto = new CanvasMemberDto();

        // Assert
        Assert.Equal(Guid.Empty, dto.Id);
        Assert.Null(dto.ProfileInfo);
        Assert.False(dto.IsMasked);
        Assert.Equal(VisibilityStatus.Hidden, dto.VisibilityStatus);
    }
}
