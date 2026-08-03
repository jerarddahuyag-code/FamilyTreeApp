using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Users.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Users.Entities;
using FluentAssertions;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Users;

public class DeleteUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ReturnsFailure()
    {
        IApplicationDbContext context = Substitute.For<IApplicationDbContext>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

        context.Users.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new DeleteUserCommandHandler(context, unitOfWork);
        var command = new DeleteUserCommand
        {
            UserId = Guid.NewGuid()
        };

        Result<bool> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.UserErrors.UserNotFound);
    }
}
