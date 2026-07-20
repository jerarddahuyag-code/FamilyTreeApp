using FamilyTreeApp.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
