using FamilyTreeApp.Application.Canvas.Commands;
using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Canvas;

public class RemoveTreeEdgeCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly RemoveTreeEdgeCommandHandler _handler;

    public RemoveTreeEdgeCommandHandlerTests()
    {
        _handler = new RemoveTreeEdgeCommandHandler(_dbContextMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task HandleAsync_EdgeExistsAndTreeIdMatches_RemovesEdgeAndReturnsSuccess()
    {
        var treeId = Guid.NewGuid();
        var edgeId = Guid.NewGuid();
        var sourceNodeId = Guid.NewGuid();
        var targetNodeId = Guid.NewGuid();

        TreeEdge edge = TreeEdge.Create(edgeId, treeId, sourceNodeId, targetNodeId).Value;
        DbSet<TreeEdge> dbSet = new List<TreeEdge> { edge }.BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(dbSet);

        var command = new RemoveTreeEdgeCommand
        {
            TreeId = treeId,
            EdgeId = edgeId
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _dbContextMock.TreeEdges.Received(1).Remove(edge);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_EdgeNotFound_ReturnsEdgeNotFoundError()
    {
        DbSet<TreeEdge> dbSet = new List<TreeEdge>().BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(dbSet);

        var command = new RemoveTreeEdgeCommand
        {
            TreeId = Guid.NewGuid(),
            EdgeId = Guid.NewGuid()
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.CanvasErrors.EdgeNotFound);
    }

    [Fact]
    public async Task HandleAsync_TreeIdMismatch_ReturnsEdgeNotFoundError()
    {
        var edgeTreeId = Guid.NewGuid();
        var commandTreeId = Guid.NewGuid();
        var edgeId = Guid.NewGuid();

        TreeEdge edge = TreeEdge.Create(edgeId, edgeTreeId, Guid.NewGuid(), Guid.NewGuid()).Value;
        DbSet<TreeEdge> dbSet = new List<TreeEdge> { edge }.BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(dbSet);

        var command = new RemoveTreeEdgeCommand
        {
            TreeId = commandTreeId,
            EdgeId = edgeId
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.CanvasErrors.EdgeNotFound);
    }
}
