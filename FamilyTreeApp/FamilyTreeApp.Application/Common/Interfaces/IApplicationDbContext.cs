using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<Tree> Trees { get; }

    DbSet<TreeRbac> TreeRbacs { get; }

    DbSet<ExternalLogin> ExternalLogins { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
