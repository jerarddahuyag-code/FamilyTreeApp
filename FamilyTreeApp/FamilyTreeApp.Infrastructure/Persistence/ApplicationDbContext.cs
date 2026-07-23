using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<Tree> Trees { get; set; }

    public DbSet<TreeRbac> TreeRbacs { get; set; }

    public DbSet<ExternalLogin> ExternalLogins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
