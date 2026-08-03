using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Trees.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Trees.Entities;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Trees;

public class UpdateTreeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenTreeNotFound_ReturnsFailure()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var emptyList = new List<Tree>();
        var mockDbSet = emptyList.BuildMockDbSet();
        context.Trees.Returns(mockDbSet);

        var handler = new UpdateTreeCommandHandler(context, unitOfWork);
        var command = new UpdateTreeCommand
        {
            TreeId = Guid.NewGuid(),
            Name = "New Name"
        };

        Result<bool> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.TreeErrors.TreeNotFound);
    }
}
