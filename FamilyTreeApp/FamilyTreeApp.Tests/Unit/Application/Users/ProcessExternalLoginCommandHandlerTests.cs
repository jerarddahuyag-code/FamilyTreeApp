using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Users.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FluentAssertions;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Users;

public class ProcessExternalLoginCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithEmptyProviderKeyOrEmail_ReturnsFailure()
    {
        IApplicationDbContext context = Substitute.For<IApplicationDbContext>();
        IAuthService authService = Substitute.For<IAuthService>();
        ICommandHandler<CreateUserCommand, Guid> createUserHandler = Substitute.For<ICommandHandler<CreateUserCommand, Guid>>();
        ICommandHandler<CreateExternalLoginCommand, Guid> createExternalLoginHandler = Substitute.For<ICommandHandler<CreateExternalLoginCommand, Guid>>();

        var handler = new ProcessExternalLoginCommandHandler(context, authService, createUserHandler, createExternalLoginHandler);
        var command = new ProcessExternalLoginCommand
        {
            Provider = "Google",
            ProviderKey = "",
            Email = "john@example.com"
        };

        Result<bool> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.ExternalLoginErrors.InvalidProviderKey);
    }
}
