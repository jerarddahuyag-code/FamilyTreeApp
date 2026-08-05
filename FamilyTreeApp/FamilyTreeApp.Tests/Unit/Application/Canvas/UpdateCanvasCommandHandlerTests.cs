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

public class UpdateCanvasCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly UpdateCanvasCommandHandler _handler;

    public UpdateCanvasCommandHandlerTests()
    {
        _handler = new UpdateCanvasCommandHandler(_dbContextMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task HandleAsync_WhenNodeNotFound_ReturnsNodeNotFoundFailure()
    {
        var treeId = Guid.NewGuid();
        TreeNode existingNode = CreateNode(treeId, 10, 20);

        DbSet<TreeNode> nodesDbSet = new List<TreeNode> { existingNode }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(nodesDbSet);

        var missingNodeId = Guid.NewGuid();
        var command = new UpdateCanvasCommand
        {
            TreeId = treeId,
            Updates = new List<NodePositionUpdate>
            {
                new(existingNode.Id, 100, 200),
                new(missingNodeId, 300, 400)
            }
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.CanvasErrors.NodeNotFound);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNodeBelongsToDifferentTree_ReturnsNodeNotFoundFailure()
    {
        var treeId = Guid.NewGuid();
        var otherTreeId = Guid.NewGuid();
        TreeNode nodeInOtherTree = CreateNode(otherTreeId, 10, 20);

        DbSet<TreeNode> nodesDbSet = new List<TreeNode> { nodeInOtherTree }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(nodesDbSet);

        var command = new UpdateCanvasCommand
        {
            TreeId = treeId,
            Updates = new List<NodePositionUpdate>
            {
                new(nodeInOtherTree.Id, 100, 200)
            }
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.CanvasErrors.NodeNotFound);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAllNodesFound_UpdatesCoordinatesAtomically()
    {
        var treeId = Guid.NewGuid();
        TreeNode node1 = CreateNode(treeId, 10, 20);
        TreeNode node2 = CreateNode(treeId, 30, 40);

        DbSet<TreeNode> nodesDbSet = new List<TreeNode> { node1, node2 }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(nodesDbSet);

        var command = new UpdateCanvasCommand
        {
            TreeId = treeId,
            Updates = new List<NodePositionUpdate>
            {
                new(node1.Id, 100.5, 200.5),
                new(node2.Id, 300.25, 400.75)
            }
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        node1.Coordinates.X.Should().Be(100.5);
        node1.Coordinates.Y.Should().Be(200.5);
        node2.Coordinates.X.Should().Be(300.25);
        node2.Coordinates.Y.Should().Be(400.75);

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdatesIsEmpty_ReturnsSuccessWithoutError()
    {
        var treeId = Guid.NewGuid();
        DbSet<TreeNode> nodesDbSet = new List<TreeNode>().BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(nodesDbSet);

        var command = new UpdateCanvasCommand
        {
            TreeId = treeId,
            Updates = new List<NodePositionUpdate>()
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static TreeNode CreateNode(Guid treeId, double x, double y)
    {
        return TreeNode.Create(
            Guid.NewGuid(),
            treeId,
            NodeType.Single,
            new CanvasCoordinates(x, y)).Value;
    }
}
