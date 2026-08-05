using FamilyTreeApp.Api.Authorization;
using FamilyTreeApp.Application.Trees.Services;
using FamilyTreeApp.Domain.Trees.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using System.Security.Claims;

namespace FamilyTreeApp.Tests.Unit.Api;

public class TreeAuthorizationHandlerTests
{
    private readonly ITreeRoleService _treeRoleService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TreeAuthorizationHandler _sut;

    public TreeAuthorizationHandlerTests()
    {
        _treeRoleService = Substitute.For<ITreeRoleService>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        _sut = new TreeAuthorizationHandler(_treeRoleService, _httpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_HttpContextIsNull_DoesNotSucceedRequirement()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var requirement = new TreeOwnerRequirement();
        var context = BuildAuthContext([requirement], Guid.NewGuid());

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_RouteDataHasNoTreeId_DoesNotSucceedRequirement()
    {
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);
        var context = BuildAuthContext([new TreeOwnerRequirement()], Guid.NewGuid());

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_UserHasNoNameIdentifierClaim_DoesNotSucceedRequirement()
    {
        var httpContext = BuildHttpContext(Guid.NewGuid(), "treeId");
        _httpContextAccessor.HttpContext.Returns(httpContext);
        var user = new ClaimsPrincipal(new ClaimsIdentity([], "Test"));
        var context = new AuthorizationHandlerContext([new TreeOwnerRequirement()], user, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_RoleIsOwner_SucceedsOwnerAdminAndMemberRequirements()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _httpContextAccessor.HttpContext.Returns(BuildHttpContext(treeId, "treeId"));
        _treeRoleService.GetUserRoleAsync(treeId, userId, Arg.Any<CancellationToken>())
            .Returns(TreeRole.Owner);

        var context = BuildAuthContext([new TreeOwnerRequirement(), new TreeAdminRequirement(), new TreeMemberRequirement()], userId);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        context.PendingRequirements.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_RoleIsAdmin_SucceedsAdminAndMemberRequirementsOnly()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _httpContextAccessor.HttpContext.Returns(BuildHttpContext(treeId, "treeId"));
        _treeRoleService.GetUserRoleAsync(treeId, userId, Arg.Any<CancellationToken>())
            .Returns(TreeRole.Admin);

        var context = BuildAuthContext([new TreeOwnerRequirement(), new TreeAdminRequirement(), new TreeMemberRequirement()], userId);

        await _sut.HandleAsync(context);

        context.PendingRequirements.Should().ContainSingle(r => r is TreeOwnerRequirement);
        context.PendingRequirements.Should().NotContain(r => r is TreeAdminRequirement);
        context.PendingRequirements.Should().NotContain(r => r is TreeMemberRequirement);
    }

    [Fact]
    public async Task HandleAsync_RoleIsMember_SucceedsMemberRequirementOnly()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _httpContextAccessor.HttpContext.Returns(BuildHttpContext(treeId, "treeId"));
        _treeRoleService.GetUserRoleAsync(treeId, userId, Arg.Any<CancellationToken>())
            .Returns(TreeRole.Member);

        var context = BuildAuthContext([new TreeOwnerRequirement(), new TreeAdminRequirement(), new TreeMemberRequirement()], userId);

        await _sut.HandleAsync(context);

        context.PendingRequirements.Should().HaveCount(2);
        context.PendingRequirements.Should().Contain(r => r is TreeOwnerRequirement);
        context.PendingRequirements.Should().Contain(r => r is TreeAdminRequirement);
        context.PendingRequirements.Should().NotContain(r => r is TreeMemberRequirement);
    }

    [Fact]
    public async Task HandleAsync_UserNotInTree_DoesNotSucceedRequirement()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _httpContextAccessor.HttpContext.Returns(BuildHttpContext(treeId, "treeId"));
        _treeRoleService.GetUserRoleAsync(treeId, userId, Arg.Any<CancellationToken>())
            .Returns((TreeRole?)null);

        var context = BuildAuthContext([new TreeMemberRequirement()], userId);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_RouteValueIsGuidType_ResolvesTreeIdCorrectly()
    {
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        // Use "id" key with a raw Guid value (not string)
        _httpContextAccessor.HttpContext.Returns(BuildHttpContext(treeId, "id", asGuid: true));
        _treeRoleService.GetUserRoleAsync(treeId, userId, Arg.Any<CancellationToken>())
            .Returns(TreeRole.Owner);

        var context = BuildAuthContext([new TreeOwnerRequirement()], userId);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static DefaultHttpContext BuildHttpContext(Guid treeId, string routeKey, bool asGuid = false)
    {
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values[routeKey] = asGuid ? (object)treeId : treeId.ToString();
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });
        return httpContext;
    }

    private static AuthorizationHandlerContext BuildAuthContext(
        IEnumerable<IAuthorizationRequirement> requirements,
        Guid userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            "Test"));
        return new AuthorizationHandlerContext(requirements.ToList(), user, null);
    }
}
