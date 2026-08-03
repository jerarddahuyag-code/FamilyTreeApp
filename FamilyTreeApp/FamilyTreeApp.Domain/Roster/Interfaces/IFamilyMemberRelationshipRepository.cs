using FamilyTreeApp.Domain.Roster.Entities;

namespace FamilyTreeApp.Domain.Roster.Interfaces;

public interface IFamilyMemberRelationshipRepository
{
    Task<FamilyMemberRelationship?> GetByIdAsync(Guid relationshipId, CancellationToken cancellationToken = default);
    Task<List<FamilyMemberRelationship>> GetByTreeIdAsync(Guid treeId, CancellationToken cancellationToken = default);
    Task<List<FamilyMemberRelationship>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid baseMemberId, Guid relatedMemberId, RelationshipType type, CancellationToken cancellationToken = default);
    void Add(FamilyMemberRelationship relationship);
    void Delete(FamilyMemberRelationship relationship);
}
