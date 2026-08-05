using FamilyTreeApp.Application.Canvas.DTOs;
using FamilyTreeApp.Application.Canvas.Queries;
using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Canvas.Enums;
using FamilyTreeApp.Domain.Canvas.Services;
using FamilyTreeApp.Domain.Canvas.ValueObjects;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Trees.Enums;
using FamilyTreeApp.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Canvas;

public class GetCanvasQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IVisibilityMediator _visibilityMediator = new VisibilityMediator();
    private readonly GetCanvasQueryHandler _handler;

    public GetCanvasQueryHandlerTests()
    {
        _handler = new GetCanvasQueryHandler(_dbContextMock, _visibilityMediator);
    }

    [Fact]
    public async Task HandleAsync_TreeHasNodesAndEdges_ReturnsCanvasDtoWithNodesAndEdges()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        TreeRbac treeRbac = TreeRbac.Create(Guid.NewGuid(), treeId, userId, TreeRole.Member).Value;
        DbSet<TreeRbac> treeRbacsDbSet = new List<TreeRbac> { treeRbac }.BuildMockDbSet();
        _dbContextMock.TreeRbacs.Returns(treeRbacsDbSet);

        var memberProfile = new ProfileInfo { FirstName = "Alice", LastName = "Smith" };
        FamilyMember member = FamilyMember.Create(Guid.NewGuid(), treeId, null, VisibilityStatus.Visible, memberProfile).Value;

        TreeNode node = TreeNode.Create(Guid.NewGuid(), treeId, NodeType.Single, new CanvasCoordinates(10, 20)).Value;
        node.Members.Add(new TreeNodeMember(node.Id, member.FamilyMemberId));

        typeof(TreeNodeMember).GetProperty(nameof(TreeNodeMember.FamilyMember))?
            .SetValue(node.Members.First(), member);

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
    public async Task HandleAsync_UserIsAdmin_ReturnsUnmaskedMemberData()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        TreeRbac treeRbac = TreeRbac.Create(Guid.NewGuid(), treeId, userId, TreeRole.Admin).Value;
        DbSet<TreeRbac> treeRbacsDbSet = new List<TreeRbac> { treeRbac }.BuildMockDbSet();
        _dbContextMock.TreeRbacs.Returns(treeRbacsDbSet);

        var memberProfile = new ProfileInfo { FirstName = "Secret", LastName = "Person" };
        FamilyMember member = FamilyMember.Create(Guid.NewGuid(), treeId, null, VisibilityStatus.Hidden, memberProfile).Value;

        TreeNode node = TreeNode.Create(Guid.NewGuid(), treeId, NodeType.Single, new CanvasCoordinates(0, 0)).Value;
        node.Members.Add(new TreeNodeMember(node.Id, member.FamilyMemberId));

        typeof(TreeNodeMember).GetProperty(nameof(TreeNodeMember.FamilyMember))?
            .SetValue(node.Members.First(), member);

        DbSet<TreeNode> treeNodesDbSet = new List<TreeNode> { node }.BuildMockDbSet();
        _dbContextMock.TreeNodes.Returns(treeNodesDbSet);

        DbSet<TreeEdge> treeEdgesDbSet = new List<TreeEdge>().BuildMockDbSet();
        _dbContextMock.TreeEdges.Returns(treeEdgesDbSet);

        var query = new GetCanvasQuery { TreeId = treeId, RequestingUserId = userId };

        Result<GetCanvasQueryResponse> result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Nodes[0].Members[0].IsMasked.Should().BeFalse();
        result.Value.Nodes[0].Members[0].ProfileInfo.FirstName.Should().Be("Secret");
    }

    [Fact]
    public async Task HandleAsync_UserIsNotAdminAndMemberIsHidden_ReturnsMaskedMemberData()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        TreeRbac treeRbac = TreeRbac.Create(Guid.NewGuid(), treeId, userId, TreeRole.Member).Value;
        DbSet<TreeRbac> treeRbacsDbSet = new List<TreeRbac> { treeRbac }.BuildMockDbSet();
        _dbContextMock.TreeRbacs.Returns(treeRbacsDbSet);

        var memberProfile = new ProfileInfo { FirstName = "Secret", LastName = "Person" };
        FamilyMember member = FamilyMember.Create(Guid.NewGuid(), treeId, null, VisibilityStatus.Hidden, memberProfile).Value;

        TreeNode node = TreeNode.Create(Guid.NewGuid(), treeId, NodeType.Single, new CanvasCoordinates(0, 0)).Value;
        node.Members.Add(new TreeNodeMember(node.Id, member.FamilyMemberId));

        typeof(TreeNodeMember).GetProperty(nameof(TreeNodeMember.FamilyMember))?
            .SetValue(node.Members.First(), member);

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

        DbSet<TreeRbac> treeRbacsDbSet = new List<TreeRbac>().BuildMockDbSet();
        _dbContextMock.TreeRbacs.Returns(treeRbacsDbSet);

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
