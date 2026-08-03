using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Users.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Users.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Users;

public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsSuccess()
    {
        IApplicationDbContext context = Substitute.For<IApplicationDbContext>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        DbSet<User> userDbSet = Substitute.For<DbSet<User>>();

        context.Users.Returns(userDbSet);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

        var handler = new CreateUserCommandHandler(context, unitOfWork);
        var command = new CreateUserCommand
        {
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsPublic = true
        };

        Result<Guid> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidEmail_ReturnsFailure()
    {
        IApplicationDbContext context = Substitute.For<IApplicationDbContext>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        DbSet<User> userDbSet = Substitute.For<DbSet<User>>();

        context.Users.Returns(userDbSet);

        var handler = new CreateUserCommandHandler(context, unitOfWork);
        var command = new CreateUserCommand
        {
            Email = "invalid-email",
            FirstName = "John",
            LastName = "Doe",
            IsPublic = true
        };

        Result<Guid> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.UserErrors.InvalidEmail);
    }
}
