using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Trees.Services;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Trees.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace FamilyTreeApp.Tests.Unit.Application.Trees;

public class TreeRoleServiceTests
{
    private readonly IApplicationDbContext _dbContext = Substitute.For<IApplicationDbContext>();
    private readonly IDistributedCache _cache = Substitute.For<IDistributedCache>();
    private readonly ILogger<TreeRoleService> _logger = Substitute.For<ILogger<TreeRoleService>>();
    private readonly TreeRoleService _sut;

    public TreeRoleServiceTests()
    {
        _sut = new TreeRoleService(_dbContext, _cache, _logger);
    }

    [Fact]
    public async Task GetUserRoleAsync_WhenRoleInCache_ReturnsCachedRoleWithoutDbCall()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expectedRole = TreeRole.Admin;
        var cachedBytes = JsonSerializer.SerializeToUtf8Bytes(expectedRole);

        _cache.GetAsync($"rbac:{treeId}:{userId}", Arg.Any<CancellationToken>())
            .Returns(cachedBytes);

        // Act
        TreeRole? result = await _sut.GetUserRoleAsync(treeId, userId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(expectedRole);
        _dbContext.TreeRbacs.DidNotReceive();
    }

    [Fact]
    public async Task GetUserRoleAsync_WhenCacheMiss_ReadsFromDbAndPopulatesCache()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _cache.GetAsync($"rbac:{treeId}:{userId}", Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        TreeRbac rbac = TreeRbac.Create(Guid.NewGuid(), treeId, userId, TreeRole.Owner).Value;
        DbSet<TreeRbac> mockDbSet = new List<TreeRbac> { rbac }.BuildMockDbSet();
        _dbContext.TreeRbacs.Returns(mockDbSet);

        // Act
        TreeRole? result = await _sut.GetUserRoleAsync(treeId, userId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(TreeRole.Owner);
        await _cache.Received(1).SetAsync(
            $"rbac:{treeId}:{userId}",
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserRoleAsync_WhenCacheMissAndUserNotInDb_ReturnsNull()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _cache.GetAsync($"rbac:{treeId}:{userId}", Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        DbSet<TreeRbac> mockDbSet = new List<TreeRbac>().BuildMockDbSet();
        _dbContext.TreeRbacs.Returns(mockDbSet);

        // Act
        TreeRole? result = await _sut.GetUserRoleAsync(treeId, userId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
        await _cache.DidNotReceive().SetAsync(
            Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserRoleAsync_WhenCacheReadThrows_FallsBackToDbAndLogs()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _cache.GetAsync($"rbac:{treeId}:{userId}", Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Redis unavailable"));

        TreeRbac rbac = TreeRbac.Create(Guid.NewGuid(), treeId, userId, TreeRole.Member).Value;
        DbSet<TreeRbac> mockDbSet = new List<TreeRbac> { rbac }.BuildMockDbSet();
        _dbContext.TreeRbacs.Returns(mockDbSet);

        // Act
        TreeRole? result = await _sut.GetUserRoleAsync(treeId, userId, TestContext.Current.CancellationToken);

        // Assert — falls back gracefully, does not throw
        result.Should().Be(TreeRole.Member);
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GetUserRoleAsync_WhenCacheWriteThrows_StillReturnsRoleAndLogs()
    {
        // Arrange
        var treeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _cache.GetAsync($"rbac:{treeId}:{userId}", Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        TreeRbac rbac = TreeRbac.Create(Guid.NewGuid(), treeId, userId, TreeRole.Admin).Value;
        DbSet<TreeRbac> mockDbSet = new List<TreeRbac> { rbac }.BuildMockDbSet();
        _dbContext.TreeRbacs.Returns(mockDbSet);

        _cache.SetAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Redis write failed"));

        // Act — must not throw despite cache failure
        TreeRole? result = await _sut.GetUserRoleAsync(treeId, userId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(TreeRole.Admin);
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
