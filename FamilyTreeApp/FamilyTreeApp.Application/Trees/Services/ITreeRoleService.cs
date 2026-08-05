using FamilyTreeApp.Domain.Trees.Enums;

namespace FamilyTreeApp.Application.Trees.Services;

/// <summary>
/// Resolves a user's role within a tree, using a distributed cache for performance.
/// Falls back to the database on cache miss and re-populates the cache.
/// </summary>
public interface ITreeRoleService
{
    /// <summary>
    /// Returns the TreeRole for the given user in the given tree, or null if the user
    /// is not a member of the tree.
    /// </summary>
    Task<TreeRole?> GetUserRoleAsync(Guid treeId, Guid userId, CancellationToken cancellationToken = default);
}
