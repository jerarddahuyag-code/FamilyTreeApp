using FamilyTreeApp.Application.Canvas.Commands;
using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Canvas.Enums;
using FamilyTreeApp.Domain.Canvas.ValueObjects;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Canvas;

public class AddTreeEdgeCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly AddTreeEdgeCommandHandler _handler;

    public AddTreeEdgeCommandHandlerTests()
    {
        _handler = new AddTreeEdgeCommandHandler(_dbContextMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task HandleAsync_BothNodesExistInSameTree_ReturnsEdgeId()
    {
        var treeId = Guid.NewGuid();
        TreeNode sourceNode = CreateNode(treeId);
        TreeNode targetNode = CreateNode(treeId);

        DbSet<TreeNode> nodesDbSet = new List<TreeNode> { sourceNode, targetNode }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(nodesDbSet);

        DbSet<TreeEdge> edgesDbSet = new List<TreeEdge>().BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(edgesDbSet);

        var command = new AddTreeEdgeCommand
        {
            TreeId = treeId,
            SourceNodeId = sourceNode.Id,
            TargetNodeId = targetNode.Id
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _dbContextMock.TreeEdges.Received(1).Add(Arg.Any<TreeEdge>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SourceNodeNotFound_ReturnsNodeNotInTreeError()
    {
        var treeId = Guid.NewGuid();
        TreeNode targetNode = CreateNode(treeId);

        DbSet<TreeNode> nodesDbSet = new List<TreeNode> { targetNode }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(nodesDbSet);

        DbSet<TreeEdge> edgesDbSet = new List<TreeEdge>().BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(edgesDbSet);

        var command = new AddTreeEdgeCommand
        {
            TreeId = treeId,
            SourceNodeId = Guid.NewGuid(),
            TargetNodeId = targetNode.Id
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.CanvasErrors.NodeNotInTree);
    }

    [Fact]
    public async Task HandleAsync_TargetNodeNotFound_ReturnsNodeNotInTreeError()
    {
        var treeId = Guid.NewGuid();
        TreeNode sourceNode = CreateNode(treeId);

        DbSet<TreeNode> nodesDbSet = new List<TreeNode> { sourceNode }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(nodesDbSet);

        DbSet<TreeEdge> edgesDbSet = new List<TreeEdge>().BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(edgesDbSet);

        var command = new AddTreeEdgeCommand
        {
            TreeId = treeId,
            SourceNodeId = sourceNode.Id,
            TargetNodeId = Guid.NewGuid()
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.CanvasErrors.NodeNotInTree);
    }

    [Fact]
    public async Task HandleAsync_SourceNodeInDifferentTree_ReturnsNodeNotInTreeError()
    {
        var treeId = Guid.NewGuid();
        TreeNode sourceNode = CreateNode(Guid.NewGuid());
        TreeNode targetNode = CreateNode(treeId);

        DbSet<TreeNode> nodesDbSet = new List<TreeNode> { sourceNode, targetNode }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(nodesDbSet);

        DbSet<TreeEdge> edgesDbSet = new List<TreeEdge>().BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(edgesDbSet);

        var command = new AddTreeEdgeCommand
        {
            TreeId = treeId,
            SourceNodeId = sourceNode.Id,
            TargetNodeId = targetNode.Id
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.CanvasErrors.NodeNotInTree);
    }

    [Fact]
    public async Task HandleAsync_TargetNodeInDifferentTree_ReturnsNodeNotInTreeError()
    {
        var treeId = Guid.NewGuid();
        TreeNode sourceNode = CreateNode(treeId);
        TreeNode targetNode = CreateNode(Guid.NewGuid());

        DbSet<TreeNode> nodesDbSet = new List<TreeNode> { sourceNode, targetNode }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(nodesDbSet);

        DbSet<TreeEdge> edgesDbSet = new List<TreeEdge>().BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(edgesDbSet);

        var command = new AddTreeEdgeCommand
        {
            TreeId = treeId,
            SourceNodeId = sourceNode.Id,
            TargetNodeId = targetNode.Id
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.CanvasErrors.NodeNotInTree);
    }

    private static TreeNode CreateNode(Guid treeId)
    {
        return TreeNode.Create(
            Guid.NewGuid(),
            treeId,
            NodeType.Single,
            new CanvasCoordinates(0, 0)).Value;
    }
}
