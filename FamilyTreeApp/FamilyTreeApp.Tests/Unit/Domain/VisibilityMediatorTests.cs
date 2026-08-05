using FamilyTreeApp.Domain.Canvas.Services;
using FamilyTreeApp.Domain.Common.ValueObjects;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.Trees.Enums;
using FluentAssertions;

namespace FamilyTreeApp.Tests.Unit.Domain;

public class VisibilityMediatorTests
{
    private static readonly ProfileInfo SampleProfile = new()
    {
        FirstName = "John",
        LastName = "Doe",
        BirthDate = new DateTime(1980, 1, 1),
        Bio = "Sample member bio"
    };

    [Fact]
    public void MaskMember_WhenVisibilityHiddenAndRequesterIsNotAdmin_ReturnsMasked()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var treeId = Guid.NewGuid();
        FamilyMember member = FamilyMember.Create(
            memberId,
            treeId,
            claimedByUserId: null,
            VisibilityStatus.Hidden,
            SampleProfile).Value;

        FamilyMember[] members = new[] { member };

        // Act - testing non-admin roles: Viewer, Editor, and null (Public)
        Dictionary<Guid, CanvasMemberVisibility> resultMember = VisibilityMediator.ResolveVisibility(members, TreeRole.Member);
        Dictionary<Guid, CanvasMemberVisibility> resultPublic = VisibilityMediator.ResolveVisibility(members, (TreeRole?)null);
        Dictionary<Guid, CanvasMemberVisibility> resultBool = VisibilityMediator.ResolveVisibility(members, isTreeAdmin: false);

        // Assert
        resultMember[memberId].IsMasked.Should().BeTrue();
        resultMember[memberId].ProfileInfo.Should().BeEquivalentTo(ProfileInfo.CreateAnonymous());
        resultMember[memberId].VisibilityStatus.Should().Be(VisibilityStatus.Hidden);

        resultPublic[memberId].IsMasked.Should().BeTrue();
        resultBool[memberId].IsMasked.Should().BeTrue();
    }

    [Fact]
    public void MaskMember_WhenVisibilityVisibleAndRequesterIsPublic_ReturnsUnmasked()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var treeId = Guid.NewGuid();
        FamilyMember member = FamilyMember.Create(
            memberId,
            treeId,
            claimedByUserId: null,
            VisibilityStatus.Visible,
            SampleProfile).Value;

        FamilyMember[] members = new[] { member };

        // Act - Public requester (null role)
        Dictionary<Guid, CanvasMemberVisibility> resultPublic = VisibilityMediator.ResolveVisibility(members, (TreeRole?)null);
        Dictionary<Guid, CanvasMemberVisibility> resultMember = VisibilityMediator.ResolveVisibility(members, TreeRole.Member);

        // Assert
        resultPublic[memberId].IsMasked.Should().BeFalse();
        resultPublic[memberId].ProfileInfo.Should().BeEquivalentTo(SampleProfile);
        resultPublic[memberId].VisibilityStatus.Should().Be(VisibilityStatus.Visible);

        resultMember[memberId].IsMasked.Should().BeFalse();
        resultMember[memberId].ProfileInfo.Should().BeEquivalentTo(SampleProfile);
    }

    [Fact]
    public void MaskMember_WhenUserIsAdminAndMemberIsHidden_ReturnsUnmasked()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var treeId = Guid.NewGuid();
        FamilyMember member = FamilyMember.Create(
            memberId,
            treeId,
            claimedByUserId: null,
            VisibilityStatus.Hidden,
            SampleProfile).Value;

        FamilyMember[] members = new[] { member };

        // Act - Admin, Owner, and isTreeAdmin: true
        Dictionary<Guid, CanvasMemberVisibility> resultAdmin = VisibilityMediator.ResolveVisibility(members, TreeRole.Admin);
        Dictionary<Guid, CanvasMemberVisibility> resultOwner = VisibilityMediator.ResolveVisibility(members, TreeRole.Owner);
        Dictionary<Guid, CanvasMemberVisibility> resultBool = VisibilityMediator.ResolveVisibility(members, isTreeAdmin: true);

        // Assert
        resultAdmin[memberId].IsMasked.Should().BeFalse();
        resultAdmin[memberId].ProfileInfo.Should().BeEquivalentTo(SampleProfile);
        resultAdmin[memberId].VisibilityStatus.Should().Be(VisibilityStatus.Hidden);

        resultOwner[memberId].IsMasked.Should().BeFalse();
        resultOwner[memberId].ProfileInfo.Should().BeEquivalentTo(SampleProfile);

        resultBool[memberId].IsMasked.Should().BeFalse();
        resultBool[memberId].ProfileInfo.Should().BeEquivalentTo(SampleProfile);
    }

    [Fact]
    public void MaskMember_WhenVisibilityPendingAndRequesterIsNotAdmin_ReturnsMasked()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var treeId = Guid.NewGuid();
        FamilyMember member = FamilyMember.Create(
            memberId,
            treeId,
            claimedByUserId: null,
            VisibilityStatus.Hidden,
            SampleProfile).Value;

        member.TransitionToVisibility(VisibilityStatus.Pending);

        FamilyMember[] members = new[] { member };

        // Act
        Dictionary<Guid, CanvasMemberVisibility> resultMember = VisibilityMediator.ResolveVisibility(members, TreeRole.Member);
        Dictionary<Guid, CanvasMemberVisibility> resultAdmin = VisibilityMediator.ResolveVisibility(members, TreeRole.Admin);

        // Assert
        resultMember[memberId].IsMasked.Should().BeTrue();
        resultMember[memberId].ProfileInfo.Should().BeEquivalentTo(ProfileInfo.CreateAnonymous());
        resultMember[memberId].VisibilityStatus.Should().Be(VisibilityStatus.Pending);

        resultAdmin[memberId].IsMasked.Should().BeFalse();
        resultAdmin[memberId].ProfileInfo.Should().BeEquivalentTo(SampleProfile);
    }

    [Fact]
    public void ResolveVisibility_WhenMembersNull_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<FamilyMember> members = null!;

        // Act
        Action act = () => VisibilityMediator.ResolveVisibility(members, isTreeAdmin: false);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ResolveVisibility_WhenDuplicateMembers_DeduplicatesResult()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var treeId = Guid.NewGuid();
        FamilyMember member = FamilyMember.Create(
            memberId,
            treeId,
            claimedByUserId: null,
            VisibilityStatus.Visible,
            SampleProfile).Value;

        FamilyMember[] members = new[] { member, member, member };

        // Act
        Dictionary<Guid, CanvasMemberVisibility> result = VisibilityMediator.ResolveVisibility(members, isTreeAdmin: false);

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(memberId);
    }
}
