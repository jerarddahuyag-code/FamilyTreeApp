using System.Security.Claims;
using System.Text.Json;
using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Trees.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FamilyTreeApp.Api.Authorization;

public class TreeAuthorizationHandler : IAuthorizationHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TreeAuthorizationHandler> _logger;

    public TreeAuthorizationHandler(
        IApplicationDbContext dbContext,
        IDistributedCache cache,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TreeAuthorizationHandler> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        RouteData routeData = httpContext.GetRouteData();
        if (!TryGetTreeId(routeData, out Guid treeId))
        {
            return;
        }

        string? userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out Guid userId))
        {
            return;
        }

        TreeRole? role = await GetUserTreeRoleAsync(treeId, userId, httpContext.RequestAborted);
        if (role == null)
        {
            return;
        }

        foreach (IAuthorizationRequirement requirement in context.Requirements)
        {
            if (requirement is TreeOwnerRequirement && role.Value == TreeRole.Owner)
            {
                context.Succeed(requirement);
            }
            else if (requirement is TreeAdminRequirement && (role.Value == TreeRole.Owner || role.Value == TreeRole.Admin))
            {
                context.Succeed(requirement);
            }
            else if (requirement is TreeMemberRequirement && (role.Value == TreeRole.Owner || role.Value == TreeRole.Admin || role.Value == TreeRole.Member))
            {
                context.Succeed(requirement);
            }
        }
    }

    private static bool TryGetTreeId(RouteData routeData, out Guid treeId)
    {
        treeId = Guid.Empty;
        if (routeData.Values.TryGetValue("treeId", out object? val) || routeData.Values.TryGetValue("id", out val))
        {
            if (val is string strVal && Guid.TryParse(strVal, out treeId))
            {
                return true;
            }
            if (val is Guid gVal)
            {
                treeId = gVal;
                return true;
            }
        }
        return false;
    }

    private async Task<TreeRole?> GetUserTreeRoleAsync(Guid treeId, Guid userId, CancellationToken cancellationToken)
    {
        string cacheKey = $"rbac:{treeId}:{userId}";
        try
        {
            byte[]? cachedBytes = await _cache.GetAsync(cacheKey, cancellationToken);
            if (cachedBytes != null)
            {
                var cachedRole = JsonSerializer.Deserialize<TreeRole>(cachedBytes);
                return cachedRole;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read RBAC role from distributed cache");
        }

        TreeRbac? rbac = await _dbContext.TreeRbacs
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.TreeId == treeId && r.UserId == userId, cancellationToken);

        if (rbac == null)
        {
            return null;
        }

        try
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(rbac.TreeRole);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetAsync(cacheKey, bytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set RBAC role in distributed cache");
        }

        return rbac.TreeRole;
    }
}
