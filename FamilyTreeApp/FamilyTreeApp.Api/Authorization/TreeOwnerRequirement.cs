using Microsoft.AspNetCore.Authorization;

namespace FamilyTreeApp.Api.Authorization;

public sealed class TreeOwnerRequirement : IAuthorizationRequirement
{
}
