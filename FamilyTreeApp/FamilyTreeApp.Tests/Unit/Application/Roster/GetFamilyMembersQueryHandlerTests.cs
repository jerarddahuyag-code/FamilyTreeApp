using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Roster.CQRS.Queries;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.ValueObjects;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Trees.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;
using System.Security.Claims;

namespace FamilyTreeApp.Tests.Unit.Application.Roster;

public class GetFamilyMembersQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly GetFamilyMembersQueryHandler _handler;

    public GetFamilyMembersQueryHandlerTests()
    {
        _handler = new GetFamilyMembersQueryHandler(_dbContextMock);
    }

    [Fact]
    public async Task HandleAsync_WithNonVisibleMemberAndNonAdmin_ReturnsMaskedProfile()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        FamilyMember member = FamilyMember.Create(
            Guid.NewGuid(),
            treeId,
            null,
            VisibilityStatus.Hidden,
            new ProfileInfo { FirstName = "Secret", LastName = "User" }).Value;

        DbSet<FamilyMember> memberDbSet = new List<FamilyMember> { member }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(memberDbSet);

        // User is not an admin
        DbSet<TreeRbac> rbacs = new List<TreeRbac>().BuildMockDbSet();
        _dbContextMock.TreeRbacs.Returns(rbacs);

        var query = new GetFamilyMembersQuery { TreeId = treeId, User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })) };

        Result<GetFamilyMembersResponse> result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].ProfileInfo.FirstName.Should().Be("Anonymous");
        result.Value.Items[0].ProfileInfo.LastName.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithNonVisibleMemberAndAdmin_ReturnsRealProfile()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        FamilyMember member = FamilyMember.Create(
            Guid.NewGuid(),
            treeId,
            null,
            VisibilityStatus.Hidden,
            new ProfileInfo { FirstName = "Secret", LastName = "User" }).Value;

        DbSet<FamilyMember> memberDbSet = new List<FamilyMember> { member }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(memberDbSet);

        // User is an admin
        TreeRbac adminRbac = TreeRbac.Create(Guid.NewGuid(), treeId, userId, TreeRole.Admin).Value;
        DbSet<TreeRbac> rbacs = new List<TreeRbac> { adminRbac }.BuildMockDbSet();
        _dbContextMock.TreeRbacs.Returns(rbacs);

        var query = new GetFamilyMembersQuery { TreeId = treeId, User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })) };

        Result<GetFamilyMembersResponse> result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].ProfileInfo.FirstName.Should().Be("Secret");
        result.Value.Items[0].ProfileInfo.LastName.Should().Be("User");
    }
}
