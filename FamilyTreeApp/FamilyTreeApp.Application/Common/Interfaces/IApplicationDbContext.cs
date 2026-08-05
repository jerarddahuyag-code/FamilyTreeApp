using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FamilyTreeApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<Tree> Trees { get; }

    DbSet<TreeRbac> TreeRbacs { get; }

    DbSet<ExternalLogin> ExternalLogins { get; }

    DbSet<FamilyMember> FamilyMembers { get; }

    DbSet<FamilyMemberRelationship> FamilyMemberRelationships { get; }

    DbSet<TreeNode> TreeNodes { get; }

    DbSet<TreeEdge> TreeEdges { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
