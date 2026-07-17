using FamilyTreeApp.Domain.Entities;

namespace FamilyTreeApp.Domain.Repositories;

public interface ISampleRepository
{
    Task<SampleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(SampleEntity sample, CancellationToken cancellationToken = default);
}
