using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Roster.CQRS.Queries;
using FamilyTreeApp.Application.Roster.DTOs;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.Roster.Interfaces;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Trees.Enums;
using FamilyTreeApp.Domain.ValueObjects;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Roster;

public class GetFamilyMembersQueryHandlerTests
{
    private readonly IFamilyMemberRepository _repositoryMock = Substitute.For<IFamilyMemberRepository>();
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly GetFamilyMembersQueryHandler _handler;

    public GetFamilyMembersQueryHandlerTests()
    {
        _handler = new GetFamilyMembersQueryHandler(_repositoryMock, _dbContextMock);
    }

    [Fact]
    public async Task HandleAsync_WithNonVisibleMemberAndNonAdmin_ReturnsMaskedProfile()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var member = FamilyMember.Create(
            Guid.NewGuid(),
            treeId,
            null,
            VisibilityStatus.Hidden,
            new ProfileInfo { FirstName = "Secret", LastName = "User" }).Value;

        _repositoryMock.GetByTreeIdAsync(treeId, Arg.Any<CancellationToken>())
            .Returns(new List<FamilyMember> { member });

        // User is not an admin
        var rbacs = new List<TreeRbac>().BuildMockDbSet();
        _dbContextMock.TreeRbacs.Returns(rbacs);

        var query = new GetFamilyMembersQuery { TreeId = treeId, UserId = userId };

        Result<List<FamilyMemberDto>> result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].ProfileInfo.FirstName.Should().Be("Anonymous");
        result.Value[0].ProfileInfo.LastName.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithNonVisibleMemberAndAdmin_ReturnsRealProfile()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var member = FamilyMember.Create(
            Guid.NewGuid(),
            treeId,
            null,
            VisibilityStatus.Hidden,
            new ProfileInfo { FirstName = "Secret", LastName = "User" }).Value;

        _repositoryMock.GetByTreeIdAsync(treeId, Arg.Any<CancellationToken>())
            .Returns(new List<FamilyMember> { member });

        // User is an admin
        var adminRbac = TreeRbac.Create(Guid.NewGuid(), treeId, userId, TreeRole.Admin).Value;
        var rbacs = new List<TreeRbac> { adminRbac }.BuildMockDbSet();
        _dbContextMock.TreeRbacs.Returns(rbacs);

        var query = new GetFamilyMembersQuery { TreeId = treeId, UserId = userId };

        Result<List<FamilyMemberDto>> result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].ProfileInfo.FirstName.Should().Be("Secret");
        result.Value[0].ProfileInfo.LastName.Should().Be("User");
    }
}
