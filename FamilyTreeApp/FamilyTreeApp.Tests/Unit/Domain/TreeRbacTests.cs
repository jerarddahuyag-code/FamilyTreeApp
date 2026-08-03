using FluentAssertions;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Trees.Enums;

namespace FamilyTreeApp.Tests.Unit.Domain;

public class TreeRbacTests
{
    [Fact]
    public void Create_WithOwnerRole_ReturnsSuccess()
    {
        var result = TreeRbac.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TreeRole.Owner);

        result.IsSuccess.Should().BeTrue();
        result.Value.TreeRole.Should().Be(TreeRole.Owner);
    }

    [Fact]
    public void Create_WithMemberRole_ReturnsSuccess()
    {
        var result = TreeRbac.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TreeRole.Member);

        result.IsSuccess.Should().BeTrue();
        result.Value.TreeRole.Should().Be(TreeRole.Member);
    }
}
