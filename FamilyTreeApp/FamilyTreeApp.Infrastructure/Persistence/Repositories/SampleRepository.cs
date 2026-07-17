using FamilyTreeApp.Domain.Entities;
using FamilyTreeApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Infrastructure.Persistence.Repositories;

public class SampleRepository(ApplicationDbContext dbContext) : ISampleRepository
{
    public async Task<SampleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<SampleEntity>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(SampleEntity sample, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<SampleEntity>().AddAsync(sample, cancellationToken);
    }
}
