using FamilyTreeApp.Api.Controllers;
using FamilyTreeApp.Application.Users.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace FamilyTreeApp.Tests.Unit.Api;

public class UsersControllerTests
{
    private readonly ICommandHandler<DeleteUserCommand, bool> _deleteUserHandler;
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
        _deleteUserHandler = Substitute.For<ICommandHandler<DeleteUserCommand, bool>>();
        _sut = new UsersController();
    }

    [Fact]
    public async Task DeleteUser_WhenUserIsDeletingSelf_ReturnsNoContent()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, targetUserId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _deleteUserHandler.HandleAsync(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        // Act
        var result = await _sut.DeleteUser(targetUserId, _deleteUserHandler, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        await _deleteUserHandler.Received(1).HandleAsync(
            Arg.Is<DeleteUserCommand>(c => c != null && c.UserId == targetUserId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteUser_WhenUserIsAdmin_ReturnsNoContent()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _deleteUserHandler.HandleAsync(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        // Act
        var result = await _sut.DeleteUser(targetUserId, _deleteUserHandler, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        await _deleteUserHandler.Received(1).HandleAsync(
            Arg.Is<DeleteUserCommand>(c => c != null && c.UserId == targetUserId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteUser_WhenUserIsNotSelfAndNotAdmin_ReturnsForbid()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _sut.DeleteUser(targetUserId, _deleteUserHandler, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ForbidResult>();
        await _deleteUserHandler.DidNotReceive().HandleAsync(
            Arg.Any<DeleteUserCommand>(),
            Arg.Any<CancellationToken>());
    }
}
