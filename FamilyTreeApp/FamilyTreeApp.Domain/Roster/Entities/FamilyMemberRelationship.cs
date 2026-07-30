using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyTreeApp.Domain.Roster.Entities;

public class FamilyMemberRelationship
{
    public Guid FamilyMemberRelationshipId { get; private set; }
    public Guid FamilyMemberId { get; private set; }
    public Guid RelatedFamilyMemberId { get; private set; }
    public RelationshipType RelationshipType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    private FamilyMemberRelationship() { }
    private FamilyMemberRelationship(Guid familyMemberRelationshipId, Guid familyMemberId, Guid relatedFamilyMemberId, RelationshipType relationshipType)
    {
        FamilyMemberRelationshipId = familyMemberRelationshipId;
        FamilyMemberId = familyMemberId;
        RelatedFamilyMemberId = relatedFamilyMemberId;
        RelationshipType = relationshipType;
        CreatedAt = UpdatedAt = DateTime.UtcNow;
    }
    public static Result<FamilyMemberRelationship> Create(Guid familyMemberRelationshipId, Guid familyMemberId, Guid relatedFamilyMemberId, RelationshipType relationshipType)
    {
        if (familyMemberId == relatedFamilyMemberId)
        {
            return Result.Failure<FamilyMemberRelationship>(DomainErrors.FamilyMemberRelationshipErrors.SameFamilyMembers);
        }

        var relationship = new FamilyMemberRelationship(familyMemberRelationshipId, familyMemberId, relatedFamilyMemberId, relationshipType);
        return Result.Success(relationship);
    }
}
