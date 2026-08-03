using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Users.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Users.Entities;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Users;

public class UpdateProfileCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ReturnsFailure()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var emptyList = new List<User>();
        var mockDbSet = emptyList.BuildMockDbSet();
        context.Users.Returns(mockDbSet);

        var handler = new UpdateProfileCommandHandler(context, unitOfWork);
        var command = new UpdateProfileCommand
        {
            UserId = Guid.NewGuid(),
            FirstName = "Jane"
        };

        Result<bool> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.UserErrors.UserNotFound);
    }
}
