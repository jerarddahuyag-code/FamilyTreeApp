using FamilyTreeApp.Application.Canvas.DTOs;
using FamilyTreeApp.Application.Canvas.Queries;
using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Trees.Services;
using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Canvas.Enums;
using FamilyTreeApp.Domain.Canvas.Services;
using FamilyTreeApp.Domain.Canvas.ValueObjects;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.ValueObjects;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.Trees.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Canvas;

public class GetCanvasQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IVisibilityService _visibilityServiceMock = Substitute.For<IVisibilityService>();
    private readonly ITreeRoleService _treeRoleServiceMock = Substitute.For<ITreeRoleService>();
    private readonly GetCanvasQueryHandler _handler;

    public GetCanvasQueryHandlerTests()
    {
        _handler = new GetCanvasQueryHandler(_dbContextMock, _visibilityServiceMock, _treeRoleServiceMock);
    }

    [Fact]
    public async Task HandleAsync_TreeHasNodesAndEdges_ReturnsCanvasDtoWithNodesAndEdges()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _treeRoleServiceMock.GetUserRoleAsync(treeId, userId, Arg.Any<CancellationToken>())
            .Returns(TreeRole.Member);

        var memberProfile = new ProfileInfo { FirstName = "Alice", LastName = "Smith" };
        FamilyMember member = FamilyMember.Create(Guid.NewGuid(), treeId, null, VisibilityStatus.Visible, memberProfile).Value;

        TreeNode node = TreeNode.Create(Guid.NewGuid(), treeId, NodeType.Single, new CanvasCoordinates(10, 20)).Value;
        node.Members.Add(new TreeNodeMember(node.Id, member.FamilyMemberId));
        typeof(TreeNodeMember).GetProperty(nameof(TreeNodeMember.FamilyMember))?
            .SetValue(node.Members.First(), member);

        var visMap = new Dictionary<Guid, CanvasMemberVisibility>
        {
            [member.FamilyMemberId] = new(member.FamilyMemberId, memberProfile, false, VisibilityStatus.Visible)
        };
        _visibilityServiceMock.ResolveForCanvas(Arg.Any<IEnumerable<TreeNode>>(), TreeRole.Member)
            .Returns(visMap);

        DbSet<TreeNode> treeNodesDbSet = new List<TreeNode> { node }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(treeNodesDbSet);

        TreeEdge edge = TreeEdge.Create(Guid.NewGuid(), treeId, node.Id, Guid.NewGuid()).Value;
        DbSet<TreeEdge> treeEdgesDbSet = new List<TreeEdge> { edge }.BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(treeEdgesDbSet);

        var query = new GetCanvasQuery { TreeId = treeId, RequestingUserId = userId };

        Result<GetCanvasQueryResponse> result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Nodes.Should().HaveCount(1);
        result.Value.Edges.Should().HaveCount(1);
        result.Value.Nodes[0].Id.Should().Be(node.Id);
        result.Value.Edges[0].Id.Should().Be(edge.Id);
    }

    [Fact]
    public async Task HandleAsync_UserIsAdmin_DelegatesAdminRoleToVisibilityService()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _treeRoleServiceMock.GetUserRoleAsync(treeId, userId, Arg.Any<CancellationToken>())
            .Returns(TreeRole.Admin);

        var memberProfile = new ProfileInfo { FirstName = "Secret", LastName = "Person" };
        FamilyMember member = FamilyMember.Create(Guid.NewGuid(), treeId, null, VisibilityStatus.Hidden, memberProfile).Value;

        TreeNode node = TreeNode.Create(Guid.NewGuid(), treeId, NodeType.Single, new CanvasCoordinates(0, 0)).Value;
        node.Members.Add(new TreeNodeMember(node.Id, member.FamilyMemberId));
        typeof(TreeNodeMember).GetProperty(nameof(TreeNodeMember.FamilyMember))?
            .SetValue(node.Members.First(), member);

        // Admin gets unmasked profile
        var visMap = new Dictionary<Guid, CanvasMemberVisibility>
        {
            [member.FamilyMemberId] = new(member.FamilyMemberId, memberProfile, false, VisibilityStatus.Hidden)
        };
        _visibilityServiceMock.ResolveForCanvas(Arg.Any<IEnumerable<TreeNode>>(), TreeRole.Admin)
            .Returns(visMap);

        DbSet<TreeNode> treeNodesDbSet = new List<TreeNode> { node }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(treeNodesDbSet);
        DbSet<TreeEdge> treeEdgesDbSet = new List<TreeEdge>().BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(treeEdgesDbSet);

        var query = new GetCanvasQuery { TreeId = treeId, RequestingUserId = userId };

        Result<GetCanvasQueryResponse> result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Nodes[0].Members[0].IsMasked.Should().BeFalse();
        result.Value.Nodes[0].Members[0].ProfileInfo.FirstName.Should().Be("Secret");

        // Verify the admin role was passed to the visibility service
        _visibilityServiceMock.Received(1).ResolveForCanvas(Arg.Any<IEnumerable<TreeNode>>(), TreeRole.Admin);
    }

    [Fact]
    public async Task HandleAsync_UserIsNotAdminAndMemberIsHidden_ReturnsMaskedMemberData()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _treeRoleServiceMock.GetUserRoleAsync(treeId, userId, Arg.Any<CancellationToken>())
            .Returns(TreeRole.Member);

        var memberProfile = new ProfileInfo { FirstName = "Secret", LastName = "Person" };
        FamilyMember member = FamilyMember.Create(Guid.NewGuid(), treeId, null, VisibilityStatus.Hidden, memberProfile).Value;

        TreeNode node = TreeNode.Create(Guid.NewGuid(), treeId, NodeType.Single, new CanvasCoordinates(0, 0)).Value;
        node.Members.Add(new TreeNodeMember(node.Id, member.FamilyMemberId));
        typeof(TreeNodeMember).GetProperty(nameof(TreeNodeMember.FamilyMember))?
            .SetValue(node.Members.First(), member);

        // Member gets masked profile
        var visMap = new Dictionary<Guid, CanvasMemberVisibility>
        {
            [member.FamilyMemberId] = new(member.FamilyMemberId, ProfileInfo.CreateAnonymous(), true, VisibilityStatus.Hidden)
        };
        _visibilityServiceMock.ResolveForCanvas(Arg.Any<IEnumerable<TreeNode>>(), TreeRole.Member)
            .Returns(visMap);

        DbSet<TreeNode> treeNodesDbSet = new List<TreeNode> { node }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(treeNodesDbSet);
        DbSet<TreeEdge> treeEdgesDbSet = new List<TreeEdge>().BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(treeEdgesDbSet);

        var query = new GetCanvasQuery { TreeId = treeId, RequestingUserId = userId };

        Result<GetCanvasQueryResponse> result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Nodes[0].Members[0].IsMasked.Should().BeTrue();
        result.Value.Nodes[0].Members[0].ProfileInfo.FirstName.Should().Be("Anonymous");
    }

    [Fact]
    public async Task HandleAsync_NoNodesOrEdgesExist_ReturnsEmptyCanvasDto()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _treeRoleServiceMock.GetUserRoleAsync(treeId, userId, Arg.Any<CancellationToken>())
            .Returns((TreeRole?)null);

        _visibilityServiceMock.ResolveForCanvas(Arg.Any<IEnumerable<TreeNode>>(), Arg.Any<TreeRole?>())
            .Returns(new Dictionary<Guid, CanvasMemberVisibility>());

        DbSet<TreeNode> treeNodesDbSet = new List<TreeNode>().BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(treeNodesDbSet);
        DbSet<TreeEdge> treeEdgesDbSet = new List<TreeEdge>().BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(treeEdgesDbSet);

        var query = new GetCanvasQuery { TreeId = treeId, RequestingUserId = userId };

        Result<GetCanvasQueryResponse> result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Nodes.Should().BeEmpty();
        result.Value.Edges.Should().BeEmpty();
    }
}
