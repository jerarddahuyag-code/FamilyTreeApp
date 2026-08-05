using FamilyTreeApp.Domain.Canvas.Services;
using FamilyTreeApp.Domain.Common.ValueObjects;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.Trees.Enums;
using FluentAssertions;

namespace FamilyTreeApp.Tests.Unit.Domain;

public class VisibilityServiceTests
{
    private static readonly ProfileInfo SampleProfile = new()
    {
        FirstName = "John",
        LastName = "Doe",
        BirthDate = new DateTime(1980, 1, 1),
        Bio = "Sample member bio"
    };

    private readonly VisibilityService _sut = new();

    // -----------------------------------------------------------------------
    // ResolveForMembers
    // -----------------------------------------------------------------------

    [Fact]
    public void ResolveForMembers_WhenVisibilityHiddenAndRequesterIsNotAdmin_ReturnsMasked()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        FamilyMember member = FamilyMember.Create(memberId, Guid.NewGuid(), null, VisibilityStatus.Hidden, SampleProfile).Value;

        // Act — non-admin roles: Member and null (public)
        Dictionary<Guid, CanvasMemberVisibility> resultMember = _sut.ResolveForMembers([member], TreeRole.Member);
        Dictionary<Guid, CanvasMemberVisibility> resultPublic = _sut.ResolveForMembers([member], null);

        // Assert
        resultMember[memberId].IsMasked.Should().BeTrue();
        resultMember[memberId].ProfileInfo.Should().BeEquivalentTo(ProfileInfo.CreateAnonymous());
        resultMember[memberId].VisibilityStatus.Should().Be(VisibilityStatus.Hidden);

        resultPublic[memberId].IsMasked.Should().BeTrue();
    }

    [Fact]
    public void ResolveForMembers_WhenVisibilityVisibleAndRequesterIsAnyRole_ReturnsUnmasked()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        FamilyMember member = FamilyMember.Create(memberId, Guid.NewGuid(), null, VisibilityStatus.Visible, SampleProfile).Value;

        // Act — public requester (null) and Member role
        Dictionary<Guid, CanvasMemberVisibility> resultPublic = _sut.ResolveForMembers([member], null);
        Dictionary<Guid, CanvasMemberVisibility> resultMember = _sut.ResolveForMembers([member], TreeRole.Member);

        // Assert
        resultPublic[memberId].IsMasked.Should().BeFalse();
        resultPublic[memberId].ProfileInfo.Should().BeEquivalentTo(SampleProfile);
        resultPublic[memberId].VisibilityStatus.Should().Be(VisibilityStatus.Visible);

        resultMember[memberId].IsMasked.Should().BeFalse();
    }

    [Theory]
    [InlineData(TreeRole.Admin)]
    [InlineData(TreeRole.Owner)]
    public void ResolveForMembers_WhenUserIsAdminAndMemberIsHidden_ReturnsUnmasked(TreeRole adminRole)
    {
        // Arrange
        var memberId = Guid.NewGuid();
        FamilyMember member = FamilyMember.Create(memberId, Guid.NewGuid(), null, VisibilityStatus.Hidden, SampleProfile).Value;

        // Act
        Dictionary<Guid, CanvasMemberVisibility> result = _sut.ResolveForMembers([member], adminRole);

        // Assert
        result[memberId].IsMasked.Should().BeFalse();
        result[memberId].ProfileInfo.Should().BeEquivalentTo(SampleProfile);
        result[memberId].VisibilityStatus.Should().Be(VisibilityStatus.Hidden);
    }

    [Fact]
    public void ResolveForMembers_WhenVisibilityIsPendingAndRequesterIsNotAdmin_ReturnsMasked()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        FamilyMember member = FamilyMember.Create(memberId, Guid.NewGuid(), null, VisibilityStatus.Hidden, SampleProfile).Value;
        member.TransitionToVisibility(VisibilityStatus.Pending);

        // Act
        Dictionary<Guid, CanvasMemberVisibility> resultMember = _sut.ResolveForMembers([member], TreeRole.Member);
        Dictionary<Guid, CanvasMemberVisibility> resultAdmin = _sut.ResolveForMembers([member], TreeRole.Admin);

        // Assert
        resultMember[memberId].IsMasked.Should().BeTrue();
        resultMember[memberId].ProfileInfo.Should().BeEquivalentTo(ProfileInfo.CreateAnonymous());
        resultMember[memberId].VisibilityStatus.Should().Be(VisibilityStatus.Pending);

        resultAdmin[memberId].IsMasked.Should().BeFalse();
        resultAdmin[memberId].ProfileInfo.Should().BeEquivalentTo(SampleProfile);
    }

    [Fact]
    public void ResolveForMembers_WhenMembersNull_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _sut.ResolveForMembers(null!, TreeRole.Member);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ResolveForMembers_WhenDuplicateMembers_DeduplicatesResult()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        FamilyMember member = FamilyMember.Create(memberId, Guid.NewGuid(), null, VisibilityStatus.Visible, SampleProfile).Value;

        // Act
        Dictionary<Guid, CanvasMemberVisibility> result = _sut.ResolveForMembers([member, member, member], null);

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(memberId);
    }
}
