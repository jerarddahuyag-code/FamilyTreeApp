using FamilyTreeApp.Domain.Roster.Entities;

namespace FamilyTreeApp.Application.Roster.DTOs;

public record RelationshipDto
{
    public required Guid FamilyMemberRelationshipId { get; init; }
    public required Guid TreeId { get; init; }
    public required Guid BaseFamilyMemberId { get; init; }
    public required Guid RelatedFamilyMemberId { get; init; }
    public required RelationshipType RelationshipType { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
