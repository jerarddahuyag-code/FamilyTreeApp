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

public class RemoveTreeNodeCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly RemoveTreeNodeCommandHandler _handler;

    public RemoveTreeNodeCommandHandlerTests()
    {
        _handler = new RemoveTreeNodeCommandHandler(_dbContextMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task HandleAsync_NodeExistsAndTreeIdMatches_RemovesNodeAndReturnsSuccess()
    {
        var treeId = Guid.NewGuid();
        var nodeId = Guid.NewGuid();

        TreeNode node = TreeNode.Create(nodeId, treeId, NodeType.Single, new CanvasCoordinates(100, 200)).Value;
        DbSet<TreeNode> dbSet = new List<TreeNode> { node }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(dbSet);

        var command = new RemoveTreeNodeCommand
        {
            TreeId = treeId,
            NodeId = nodeId
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _dbContextMock.TreeNodes.Received(1).Remove(node);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NodeNotFound_ReturnsNodeNotFoundError()
    {
        DbSet<TreeNode> dbSet = new List<TreeNode>().BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(dbSet);

        var command = new RemoveTreeNodeCommand
        {
            TreeId = Guid.NewGuid(),
            NodeId = Guid.NewGuid()
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.CanvasErrors.NodeNotFound);
    }

    [Fact]
    public async Task HandleAsync_TreeIdMismatch_ReturnsNodeNotFoundError()
    {
        var nodeTreeId = Guid.NewGuid();
        var commandTreeId = Guid.NewGuid();
        var nodeId = Guid.NewGuid();

        TreeNode node = TreeNode.Create(nodeId, nodeTreeId, NodeType.Single, new CanvasCoordinates(0, 0)).Value;
        DbSet<TreeNode> dbSet = new List<TreeNode> { node }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(dbSet);

        var command = new RemoveTreeNodeCommand
        {
            TreeId = commandTreeId,
            NodeId = nodeId
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.CanvasErrors.NodeNotFound);
    }
}
