using FamilyTreeApp.Api.Controllers;
using FamilyTreeApp.Application.Canvas.Commands;
using FamilyTreeApp.Application.Canvas.DTOs;
using FamilyTreeApp.Application.Canvas.Queries;
using FamilyTreeApp.Domain.Canvas.Enums;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace FamilyTreeApp.Tests.Unit.Api;

public class CanvasControllerTests
{
    private readonly CanvasController _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public CanvasControllerTests()
    {
        _sut = new CanvasController();

        Claim[] claims = [new Claim(ClaimTypes.NameIdentifier, _userId.ToString())];
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact(DisplayName = "GetCanvas returns Ok with CanvasDto when handler succeeds")]
    public async Task GetCanvas_WhenSuccessful_ReturnsOkWithCanvasDto()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        IQueryHandler<GetCanvasQuery, GetCanvasQueryResponse> handler = Substitute.For<IQueryHandler<GetCanvasQuery, GetCanvasQueryResponse>>();
        var expectedDto = new GetCanvasQueryResponse([], []);

        handler.HandleAsync(Arg.Any<GetCanvasQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedDto));

        // Act
        IActionResult result = await _sut.GetCanvas(treeId, handler, CancellationToken.None);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value!.Should().Be(expectedDto);

        await handler.Received(1).HandleAsync(
            Arg.Is<GetCanvasQuery>(q => q != null && q.TreeId == treeId && q.RequestingUserId == _userId),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "GetCanvas returns NotFound when handler fails with TreeNotFound error")]
    public async Task GetCanvas_WhenFailure_ReturnsFailureResponse()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        IQueryHandler<GetCanvasQuery, GetCanvasQueryResponse> handler = Substitute.For<IQueryHandler<GetCanvasQuery, GetCanvasQueryResponse>>();

        handler.HandleAsync(Arg.Any<GetCanvasQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<GetCanvasQueryResponse>(DomainErrors.TreeErrors.TreeNotFound));

        // Act
        IActionResult result = await _sut.GetCanvas(treeId, handler, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "UpdateCanvas returns NoContent when handler succeeds")]
    public async Task UpdateCanvas_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        ICommandHandler<UpdateCanvasCommand, bool> handler = Substitute.For<ICommandHandler<UpdateCanvasCommand, bool>>();
        var command = new UpdateCanvasCommand
        {
            TreeId = Guid.Empty,
            Updates = [new NodePositionUpdate(Guid.NewGuid(), 10.0, 20.0)]
        };

        handler.HandleAsync(Arg.Any<UpdateCanvasCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        // Act
        IActionResult result = await _sut.UpdateCanvas(treeId, command, handler, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        await handler.Received(1).HandleAsync(
            Arg.Is<UpdateCanvasCommand>(c => c != null && c.TreeId == treeId && c.Updates.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "UpdateCanvas returns NotFound when handler fails with NodeNotFound error")]
    public async Task UpdateCanvas_WhenFailure_ReturnsFailureResponse()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        ICommandHandler<UpdateCanvasCommand, bool> handler = Substitute.For<ICommandHandler<UpdateCanvasCommand, bool>>();
        var command = new UpdateCanvasCommand
        {
            TreeId = treeId,
            Updates = [new NodePositionUpdate(Guid.NewGuid(), 10.0, 20.0)]
        };

        handler.HandleAsync(Arg.Any<UpdateCanvasCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<bool>(DomainErrors.CanvasErrors.NodeNotFound));

        // Act
        IActionResult result = await _sut.UpdateCanvas(treeId, command, handler, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "AddTreeNode returns Created with NodeId when handler succeeds")]
    public async Task AddTreeNode_WhenSuccessful_ReturnsCreatedWithNodeId()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var newNodeId = Guid.NewGuid();
        ICommandHandler<AddTreeNodeCommand, Guid> handler = Substitute.For<ICommandHandler<AddTreeNodeCommand, Guid>>();
        var command = new AddTreeNodeCommand
        {
            TreeId = Guid.Empty,
            NodeType = NodeType.Single,
            X = 100.0,
            Y = 200.0
        };

        handler.HandleAsync(Arg.Any<AddTreeNodeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newNodeId));

        // Act
        IActionResult result = await _sut.AddTreeNode(treeId, command, handler, CancellationToken.None);

        // Assert
        CreatedResult createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location!.Should().Be($"/api/v1/trees/{treeId}/canvas/nodes/{newNodeId}");

        await handler.Received(1).HandleAsync(
            Arg.Is<AddTreeNodeCommand>(c => c != null && c.TreeId == treeId && c.NodeType == NodeType.Single),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "AddTreeNode returns BadRequest when handler fails with validation error")]
    public async Task AddTreeNode_WhenFailure_ReturnsFailureResponse()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        ICommandHandler<AddTreeNodeCommand, Guid> handler = Substitute.For<ICommandHandler<AddTreeNodeCommand, Guid>>();
        var command = new AddTreeNodeCommand
        {
            TreeId = treeId,
            NodeType = NodeType.Single,
            X = 0,
            Y = 0
        };

        handler.HandleAsync(Arg.Any<AddTreeNodeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(DomainErrors.CanvasErrors.MemberNotInTree));

        // Act
        IActionResult result = await _sut.AddTreeNode(treeId, command, handler, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "RemoveTreeNode returns NoContent when handler succeeds")]
    public async Task RemoveTreeNode_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var nodeId = Guid.NewGuid();
        ICommandHandler<RemoveTreeNodeCommand, bool> handler = Substitute.For<ICommandHandler<RemoveTreeNodeCommand, bool>>();

        handler.HandleAsync(Arg.Any<RemoveTreeNodeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        // Act
        IActionResult result = await _sut.RemoveTreeNode(treeId, nodeId, handler, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        await handler.Received(1).HandleAsync(
            Arg.Is<RemoveTreeNodeCommand>(c => c != null && c.TreeId == treeId && c.NodeId == nodeId),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "RemoveTreeNode returns NotFound when handler fails")]
    public async Task RemoveTreeNode_WhenFailure_ReturnsFailureResponse()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var nodeId = Guid.NewGuid();
        ICommandHandler<RemoveTreeNodeCommand, bool> handler = Substitute.For<ICommandHandler<RemoveTreeNodeCommand, bool>>();

        handler.HandleAsync(Arg.Any<RemoveTreeNodeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<bool>(DomainErrors.CanvasErrors.NodeNotFound));

        // Act
        IActionResult result = await _sut.RemoveTreeNode(treeId, nodeId, handler, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "AddTreeEdge returns Created with EdgeId when handler succeeds")]
    public async Task AddTreeEdge_WhenSuccessful_ReturnsCreatedWithEdgeId()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var newEdgeId = Guid.NewGuid();
        var sourceNodeId = Guid.NewGuid();
        var targetNodeId = Guid.NewGuid();
        ICommandHandler<AddTreeEdgeCommand, Guid> handler = Substitute.For<ICommandHandler<AddTreeEdgeCommand, Guid>>();
        var command = new AddTreeEdgeCommand
        {
            TreeId = Guid.Empty,
            SourceNodeId = sourceNodeId,
            TargetNodeId = targetNodeId
        };

        handler.HandleAsync(Arg.Any<AddTreeEdgeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newEdgeId));

        // Act
        IActionResult result = await _sut.AddTreeEdge(treeId, command, handler, CancellationToken.None);

        // Assert
        CreatedResult createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location!.Should().Be($"/api/v1/trees/{treeId}/canvas/edges/{newEdgeId}");

        await handler.Received(1).HandleAsync(
            Arg.Is<AddTreeEdgeCommand>(c => c != null && c.TreeId == treeId && c.SourceNodeId == sourceNodeId && c.TargetNodeId == targetNodeId),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "AddTreeEdge returns BadRequest when handler fails with NodeNotInTree error")]
    public async Task AddTreeEdge_WhenFailure_ReturnsFailureResponse()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        ICommandHandler<AddTreeEdgeCommand, Guid> handler = Substitute.For<ICommandHandler<AddTreeEdgeCommand, Guid>>();
        var command = new AddTreeEdgeCommand
        {
            TreeId = treeId,
            SourceNodeId = Guid.NewGuid(),
            TargetNodeId = Guid.NewGuid()
        };

        handler.HandleAsync(Arg.Any<AddTreeEdgeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(DomainErrors.CanvasErrors.NodeNotInTree));

        // Act
        IActionResult result = await _sut.AddTreeEdge(treeId, command, handler, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "RemoveTreeEdge returns NoContent when handler succeeds")]
    public async Task RemoveTreeEdge_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var edgeId = Guid.NewGuid();
        ICommandHandler<RemoveTreeEdgeCommand, bool> handler = Substitute.For<ICommandHandler<RemoveTreeEdgeCommand, bool>>();

        handler.HandleAsync(Arg.Any<RemoveTreeEdgeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        // Act
        IActionResult result = await _sut.RemoveTreeEdge(treeId, edgeId, handler, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        await handler.Received(1).HandleAsync(
            Arg.Is<RemoveTreeEdgeCommand>(c => c != null && c.TreeId == treeId && c.EdgeId == edgeId),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "RemoveTreeEdge returns NotFound when handler fails with EdgeNotFound error")]
    public async Task RemoveTreeEdge_WhenFailure_ReturnsFailureResponse()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var edgeId = Guid.NewGuid();
        ICommandHandler<RemoveTreeEdgeCommand, bool> handler = Substitute.For<ICommandHandler<RemoveTreeEdgeCommand, bool>>();

        handler.HandleAsync(Arg.Any<RemoveTreeEdgeCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<bool>(DomainErrors.CanvasErrors.EdgeNotFound));

        // Act
        IActionResult result = await _sut.RemoveTreeEdge(treeId, edgeId, handler, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
