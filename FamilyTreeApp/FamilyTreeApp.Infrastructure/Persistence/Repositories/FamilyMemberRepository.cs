using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Infrastructure.Persistence.Repositories;

public class FamilyMemberRepository(IApplicationDbContext dbContext) : IFamilyMemberRepository
{
    public async Task<FamilyMember?> GetByIdAsync(Guid familyMemberId, CancellationToken cancellationToken = default)
    {
        return await dbContext.FamilyMembers
            .Include(m => m.Relationships)
            .FirstOrDefaultAsync(m => m.FamilyMemberId == familyMemberId, cancellationToken);
    }

    public async Task<List<FamilyMember>> GetByTreeIdAsync(Guid treeId, CancellationToken cancellationToken = default)
    {
        return await dbContext.FamilyMembers
            .Include(m => m.Relationships)
            .Where(m => m.TreeId == treeId)
            .ToListAsync(cancellationToken);
    }

    public void Add(FamilyMember familyMember)
    {
        dbContext.FamilyMembers.Add(familyMember);
    }

    public void Update(FamilyMember familyMember)
    {
        dbContext.FamilyMembers.Update(familyMember);
    }

    public void Delete(FamilyMember familyMember)
    {
        dbContext.FamilyMembers.Remove(familyMember);
    }
}
