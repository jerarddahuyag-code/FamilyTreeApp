using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Users.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Users.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Users;

public class CreateExternalLoginCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsSuccess()
    {
        IApplicationDbContext context = Substitute.For<IApplicationDbContext>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        DbSet<ExternalLogin> loginsDbSet = Substitute.For<DbSet<ExternalLogin>>();

        context.ExternalLogins.Returns(loginsDbSet);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

        var handler = new CreateExternalLoginCommandHandler(context, unitOfWork);
        var command = new CreateExternalLoginCommand
        {
            ExternalLoginId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Provider = "Google",
            ProviderKey = "sub-12345"
        };

        Result<Guid> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(command.ExternalLoginId);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyProvider_ReturnsFailure()
    {
        IApplicationDbContext context = Substitute.For<IApplicationDbContext>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new CreateExternalLoginCommandHandler(context, unitOfWork);
        var command = new CreateExternalLoginCommand
        {
            ExternalLoginId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Provider = "   ",
            ProviderKey = "sub-12345"
        };

        Result<Guid> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.ExternalLoginErrors.InvalidProvider);
    }
}
