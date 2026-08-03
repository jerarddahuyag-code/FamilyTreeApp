using System.Security.Claims;
using System.Text.Json;
using FamilyTreeApp.Api.Authorization;
using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Trees.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Api;

public class TreeAuthorizationHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TreeAuthorizationHandler> _logger;
    private readonly TreeAuthorizationHandler _sut;

    public TreeAuthorizationHandlerTests()
    {
        _dbContext = Substitute.For<IApplicationDbContext>();
        _cache = Substitute.For<IDistributedCache>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _logger = Substitute.For<ILogger<TreeAuthorizationHandler>>();

        _sut = new TreeAuthorizationHandler(_dbContext, _cache, _httpContextAccessor, _logger);
    }

    [Fact]
    public async Task HandleAsync_HttpContextIsNull_DoesNotSucceedRequirement()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var requirement = new TreeOwnerRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())], "Test"));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_RouteDataHasNoTreeId_DoesNotSucceedRequirement()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);
        var requirement = new TreeOwnerRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())], "Test"));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_UserHasNoNameIdentifierClaim_DoesNotSucceedRequirement()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["treeId"] = Guid.NewGuid().ToString();
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });
        _httpContextAccessor.HttpContext.Returns(httpContext);
        var requirement = new TreeOwnerRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity([], "Test"));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_RoleInCacheIsOwner_SucceedsOwnerAdminAndMemberRequirements()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["treeId"] = treeId.ToString();
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });
        _httpContextAccessor.HttpContext.Returns(httpContext);

        byte[] cachedBytes = JsonSerializer.SerializeToUtf8Bytes(TreeRole.Owner);
        _cache.GetAsync($"rbac:{treeId}:{userId}", Arg.Any<CancellationToken>())
            .Returns(cachedBytes);

        var ownerReq = new TreeOwnerRequirement();
        var adminReq = new TreeAdminRequirement();
        var memberReq = new TreeMemberRequirement();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"));
        var context = new AuthorizationHandlerContext([ownerReq, adminReq, memberReq], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.PendingRequirements.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_RoleInCacheIsAdmin_SucceedsAdminAndMemberRequirementsOnly()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["treeId"] = treeId.ToString();
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });
        _httpContextAccessor.HttpContext.Returns(httpContext);

        byte[] cachedBytes = JsonSerializer.SerializeToUtf8Bytes(TreeRole.Admin);
        _cache.GetAsync($"rbac:{treeId}:{userId}", Arg.Any<CancellationToken>())
            .Returns(cachedBytes);

        var ownerReq = new TreeOwnerRequirement();
        var adminReq = new TreeAdminRequirement();
        var memberReq = new TreeMemberRequirement();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"));
        var context = new AuthorizationHandlerContext([ownerReq, adminReq, memberReq], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.PendingRequirements.Should().ContainSingle(r => r is TreeOwnerRequirement);
        context.PendingRequirements.Should().NotContain(r => r is TreeAdminRequirement);
        context.PendingRequirements.Should().NotContain(r => r is TreeMemberRequirement);
    }

    [Fact]
    public async Task HandleAsync_RoleInCacheIsMember_SucceedsMemberRequirementOnly()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["treeId"] = treeId.ToString();
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });
        _httpContextAccessor.HttpContext.Returns(httpContext);

        byte[] cachedBytes = JsonSerializer.SerializeToUtf8Bytes(TreeRole.Member);
        _cache.GetAsync($"rbac:{treeId}:{userId}", Arg.Any<CancellationToken>())
            .Returns(cachedBytes);

        var ownerReq = new TreeOwnerRequirement();
        var adminReq = new TreeAdminRequirement();
        var memberReq = new TreeMemberRequirement();

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"));
        var context = new AuthorizationHandlerContext([ownerReq, adminReq, memberReq], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.PendingRequirements.Should().HaveCount(2);
        context.PendingRequirements.Should().Contain(r => r is TreeOwnerRequirement);
        context.PendingRequirements.Should().Contain(r => r is TreeAdminRequirement);
        context.PendingRequirements.Should().NotContain(r => r is TreeMemberRequirement);
    }

    [Fact]
    public async Task HandleAsync_CacheMiss_ReadsFromDbAndSetsCache()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["id"] = treeId; // Test route value 'id' as Guid
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });
        _httpContextAccessor.HttpContext.Returns(httpContext);

        _cache.GetAsync($"rbac:{treeId}:{userId}", Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        TreeRbac rbac = TreeRbac.Create(Guid.NewGuid(), treeId, userId, TreeRole.Owner).Value;
        var rbacList = new List<TreeRbac> { rbac };
        DbSet<TreeRbac> mockDbSet = rbacList.BuildMockDbSet();
        _dbContext.TreeRbacs.Returns(mockDbSet);

        var ownerReq = new TreeOwnerRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"));
        var context = new AuthorizationHandlerContext([ownerReq], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        await _cache.Received(1).SetAsync(
            $"rbac:{treeId}:{userId}",
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CacheMissAndNotFoundInDb_DoesNotSucceedRequirement()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["treeId"] = treeId.ToString();
        httpContext.Features.Set<IRoutingFeature>(new RoutingFeature { RouteData = routeData });
        _httpContextAccessor.HttpContext.Returns(httpContext);

        _cache.GetAsync($"rbac:{treeId}:{userId}", Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        var emptyList = new List<TreeRbac>();
        DbSet<TreeRbac> mockDbSet = emptyList.BuildMockDbSet();
        _dbContext.TreeRbacs.Returns(mockDbSet);

        var memberReq = new TreeMemberRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"));
        var context = new AuthorizationHandlerContext([memberReq], user, null);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
