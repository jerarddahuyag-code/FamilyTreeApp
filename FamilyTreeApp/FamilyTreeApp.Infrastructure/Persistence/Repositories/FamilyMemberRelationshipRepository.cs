using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Infrastructure.Persistence.Repositories;

public class FamilyMemberRelationshipRepository(IApplicationDbContext dbContext) : IFamilyMemberRelationshipRepository
{
    public async Task<FamilyMemberRelationship?> GetByIdAsync(Guid relationshipId, CancellationToken cancellationToken = default)
    {
        return await dbContext.FamilyMemberRelationships
            .FirstOrDefaultAsync(r => r.FamilyMemberRelationshipId == relationshipId, cancellationToken);
    }

    public async Task<List<FamilyMemberRelationship>> GetByTreeIdAsync(Guid treeId, CancellationToken cancellationToken = default)
    {
        return await dbContext.FamilyMemberRelationships
            .Where(r => r.TreeId == treeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FamilyMemberRelationship>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await dbContext.FamilyMemberRelationships
            .Where(r => r.BaseFamilyMemberId == memberId || r.RelatedFamilyMemberId == memberId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid baseMemberId, Guid relatedMemberId, RelationshipType type, CancellationToken cancellationToken = default)
    {
        return await dbContext.FamilyMemberRelationships
            .AnyAsync(r => r.BaseFamilyMemberId == baseMemberId &&
                           r.RelatedFamilyMemberId == relatedMemberId &&
                           r.RelationshipType == type, cancellationToken);
    }

    public void Add(FamilyMemberRelationship relationship)
    {
        dbContext.FamilyMemberRelationships.Add(relationship);
    }

    public void Delete(FamilyMemberRelationship relationship)
    {
        dbContext.FamilyMemberRelationships.Remove(relationship);
    }
}
