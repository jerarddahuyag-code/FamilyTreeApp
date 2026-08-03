using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Trees.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Trees.Entities;
using FluentAssertions;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Trees;

public class DeleteTreeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenTreeNotFound_ReturnsFailure()
    {
        IApplicationDbContext context = Substitute.For<IApplicationDbContext>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

        context.Trees.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns((Tree?)null);

        var handler = new DeleteTreeCommandHandler(context, unitOfWork);
        var command = new DeleteTreeCommand
        {
            TreeId = Guid.NewGuid()
        };

        Result<bool> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
