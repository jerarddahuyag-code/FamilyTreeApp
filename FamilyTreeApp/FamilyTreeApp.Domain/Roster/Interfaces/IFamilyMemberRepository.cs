using FamilyTreeApp.Domain.Roster.Entities;

namespace FamilyTreeApp.Domain.Roster.Interfaces;

public interface IFamilyMemberRepository
{
    Task<FamilyMember?> GetByIdAsync(Guid familyMemberId, CancellationToken cancellationToken = default);
    Task<List<FamilyMember>> GetByTreeIdAsync(Guid treeId, CancellationToken cancellationToken = default);
    void Add(FamilyMember familyMember);
    void Update(FamilyMember familyMember);
    void Delete(FamilyMember familyMember);
}
