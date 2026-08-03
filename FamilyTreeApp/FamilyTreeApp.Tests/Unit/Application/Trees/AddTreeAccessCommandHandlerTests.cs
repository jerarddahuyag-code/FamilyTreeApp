using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Trees.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Trees.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Trees;

public class AddTreeAccessCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsSuccess()
    {
        IApplicationDbContext context = Substitute.For<IApplicationDbContext>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        DbSet<TreeRbac> rbacDbSet = Substitute.For<DbSet<TreeRbac>>();

        context.TreeRbacs.Returns(rbacDbSet);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

        var handler = new AddTreeAccessCommandHandler(context, unitOfWork);
        var command = new AddTreeAccessCommand
        {
            TreeId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AccessLevel = TreeRole.Member
        };

        Result<Guid> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }
}
