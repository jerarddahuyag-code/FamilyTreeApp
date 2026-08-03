using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Trees.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Trees.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Trees;

public class RemoveTreeAccessCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenAccessNotFound_ReturnsFailure()
    {
        IApplicationDbContext context = Substitute.For<IApplicationDbContext>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        DbSet<TreeRbac> rbacDbSet = Substitute.For<DbSet<TreeRbac>>();

        context.TreeRbacs.Returns(rbacDbSet);
        rbacDbSet.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>()).Returns((TreeRbac?)null);

        var handler = new RemoveTreeAccessCommandHandler(context, unitOfWork);
        var command = new RemoveTreeAccessCommand
        {
            TreeId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        Result<bool> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
