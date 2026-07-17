using FamilyTreeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<SampleEntity> Samples { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
