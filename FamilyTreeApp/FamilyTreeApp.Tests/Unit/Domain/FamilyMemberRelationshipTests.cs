using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FluentAssertions;

namespace FamilyTreeApp.Tests.Unit.Domain;

public class FamilyMemberRelationshipTests
{
    [Fact]
    public void Create_ValidParameters_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var treeId = Guid.NewGuid();
        var baseMemberId = Guid.NewGuid();
        var relatedMemberId = Guid.NewGuid();

        Result<FamilyMemberRelationship> result = FamilyMemberRelationship.Create(
            id,
            treeId,
            baseMemberId,
            relatedMemberId,
            RelationshipType.Parent);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FamilyMemberRelationshipId.Should().Be(id);
        result.Value.TreeId.Should().Be(treeId);
        result.Value.BaseFamilyMemberId.Should().Be(baseMemberId);
        result.Value.RelatedFamilyMemberId.Should().Be(relatedMemberId);
        result.Value.RelationshipType.Should().Be(RelationshipType.Parent);
    }

    [Fact]
    public void Create_SameBaseAndRelatedMember_ReturnsSameFamilyMembersError()
    {
        var memberId = Guid.NewGuid();

        Result<FamilyMemberRelationship> result = FamilyMemberRelationship.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            memberId,
            memberId,
            RelationshipType.Spouse);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberRelationshipErrors.SameFamilyMembers);
    }
}
