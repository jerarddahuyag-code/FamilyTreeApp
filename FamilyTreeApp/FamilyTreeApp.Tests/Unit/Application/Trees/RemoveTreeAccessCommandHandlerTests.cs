using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Trees.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Trees.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;
namespace FamilyTreeApp.Tests.Unit.Application.Trees;

public class RemoveTreeAccessCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenAccessNotFound_ReturnsFailure()
    {
        IApplicationDbContext context = Substitute.For<IApplicationDbContext>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        DbSet<TreeRbac> rbacDbSet = new List<TreeRbac>().BuildMockDbSet();
        context.TreeRbacs.Returns(rbacDbSet);
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
