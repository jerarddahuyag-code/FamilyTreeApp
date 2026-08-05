using FamilyTreeApp.Application.Canvas.Commands;
using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Canvas.Enums;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Canvas;

public class AddTreeNodeCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly AddTreeNodeCommandHandler _handler;

    public AddTreeNodeCommandHandlerTests()
    {
        _handler = new AddTreeNodeCommandHandler(_dbContextMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task HandleAsync_ValidMembersInSameTree_CreatesNodeAndLinksAndReturnsId()
    {
        var treeId = Guid.NewGuid();
        FamilyMember member1 = CreateFamilyMember(treeId);
        FamilyMember member2 = CreateFamilyMember(treeId);

        DbSet<FamilyMember> membersDbSet = new List<FamilyMember> { member1, member2 }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(membersDbSet);

        DbSet<TreeNode> nodesDbSet = new List<TreeNode>().BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(nodesDbSet);

        var command = new AddTreeNodeCommand
        {
            TreeId = treeId,
            NodeType = NodeType.Partner,
            X = 100.5,
            Y = 200.5,
            FamilyMemberIds = new[] { member1.FamilyMemberId, member2.FamilyMemberId }
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _dbContextMock.TreeNodes.Received(1).Add(Arg.Is<TreeNode>(n =>
            n != null &&
            n.TreeId == treeId &&
            n.NodeType == NodeType.Partner &&
            n.Coordinates.X == 100.5 &&
            n.Coordinates.Y == 200.5 &&
            n.Members.Count == 2));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_MemberNotFound_ReturnsMemberNotInTreeError()
    {
        var treeId = Guid.NewGuid();
        FamilyMember member1 = CreateFamilyMember(treeId);

        DbSet<FamilyMember> membersDbSet = new List<FamilyMember> { member1 }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(membersDbSet);

        var command = new AddTreeNodeCommand
        {
            TreeId = treeId,
            NodeType = NodeType.Single,
            X = 0,
            Y = 0,
            FamilyMemberIds = new[] { member1.FamilyMemberId, Guid.NewGuid() }
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.CanvasErrors.MemberNotInTree);
    }

    [Fact]
    public async Task HandleAsync_MemberInDifferentTree_ReturnsMemberNotInTreeError()
    {
        var treeId = Guid.NewGuid();
        FamilyMember memberInSameTree = CreateFamilyMember(treeId);
        FamilyMember memberInOtherTree = CreateFamilyMember(Guid.NewGuid());

        DbSet<FamilyMember> membersDbSet = new List<FamilyMember> { memberInSameTree, memberInOtherTree }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(membersDbSet);

        var command = new AddTreeNodeCommand
        {
            TreeId = treeId,
            NodeType = NodeType.Partner,
            X = 0,
            Y = 0,
            FamilyMemberIds = new[] { memberInSameTree.FamilyMemberId, memberInOtherTree.FamilyMemberId }
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.CanvasErrors.MemberNotInTree);
    }

    [Fact]
    public async Task HandleAsync_EmptyFamilyMemberIds_CreatesNodeWithoutMembersAndReturnsId()
    {
        var treeId = Guid.NewGuid();

        DbSet<FamilyMember> membersDbSet = new List<FamilyMember>().BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(membersDbSet);

        DbSet<TreeNode> nodesDbSet = new List<TreeNode>().BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(nodesDbSet);

        var command = new AddTreeNodeCommand
        {
            TreeId = treeId,
            NodeType = NodeType.Single,
            X = 10,
            Y = 20,
            FamilyMemberIds = Array.Empty<Guid>()
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _dbContextMock.TreeNodes.Received(1).Add(Arg.Is<TreeNode>(n =>
            n != null &&
            n.TreeId == treeId &&
            n.Members.Count == 0));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DuplicateFamilyMemberIdsProvided_DeduplicatesAndCreatesNodeAndLinks()
    {
        var treeId = Guid.NewGuid();
        FamilyMember member = CreateFamilyMember(treeId);

        DbSet<FamilyMember> membersDbSet = new List<FamilyMember> { member }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(membersDbSet);

        DbSet<TreeNode> nodesDbSet = new List<TreeNode>().BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(nodesDbSet);

        var command = new AddTreeNodeCommand
        {
            TreeId = treeId,
            NodeType = NodeType.Single,
            X = 50,
            Y = 50,
            FamilyMemberIds = new[] { member.FamilyMemberId, member.FamilyMemberId }
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _dbContextMock.TreeNodes.Received(1).Add(Arg.Is<TreeNode>(n => n != null && n.Members.Count == 1));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static FamilyMember CreateFamilyMember(Guid treeId)
    {
        var profile = new ProfileInfo { FirstName = "John", LastName = "Doe" };
        return FamilyMember.Create(
            Guid.NewGuid(),
            treeId,
            null,
            VisibilityStatus.Visible,
            profile).Value;
    }
}
