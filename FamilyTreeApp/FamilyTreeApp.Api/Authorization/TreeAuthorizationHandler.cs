using FamilyTreeApp.Application.Trees.Services;
using FamilyTreeApp.Domain.Trees.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FamilyTreeApp.Api.Authorization;

public class TreeAuthorizationHandler(
    ITreeRoleService treeRoleService,
    IHttpContextAccessor httpContextAccessor)
    : IAuthorizationHandler
{
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        RouteData routeData = httpContext.GetRouteData();
        if (!TryGetTreeId(routeData, out Guid treeId))
        {
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out Guid userId))
        {
            return;
        }

        TreeRole? role = await treeRoleService.GetUserRoleAsync(treeId, userId, httpContext.RequestAborted);
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
        if (routeData.Values.TryGetValue("treeId", out var val) || routeData.Values.TryGetValue("id", out val))
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
}
