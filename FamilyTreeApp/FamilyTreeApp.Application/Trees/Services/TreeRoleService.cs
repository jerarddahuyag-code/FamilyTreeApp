using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Trees.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FamilyTreeApp.Application.Trees.Services;

public class TreeRoleService(
    IApplicationDbContext dbContext,
    IDistributedCache cache,
    ILogger<TreeRoleService> logger) : ITreeRoleService
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    /// <inheritdoc/>
    public async Task<TreeRole?> GetUserRoleAsync(
        Guid treeId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"rbac:{treeId}:{userId}";

        try
        {
            var cachedBytes = await cache.GetAsync(cacheKey, cancellationToken);
            if (cachedBytes != null)
            {
                return JsonSerializer.Deserialize<TreeRole>(cachedBytes);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read RBAC role from distributed cache for tree {TreeId}, user {UserId}", treeId, userId);
        }

        TreeRbac? rbac = await dbContext.TreeRbacs
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.TreeId == treeId && r.UserId == userId, cancellationToken);

        if (rbac == null)
        {
            return null;
        }

        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(rbac.TreeRole);
            await cache.SetAsync(cacheKey, bytes, CacheOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to set RBAC role in distributed cache for tree {TreeId}, user {UserId}", treeId, userId);
        }

        return rbac.TreeRole;
    }
}
