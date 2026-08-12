using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Trees.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Trees.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Trees;

public class CreateTreeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsSuccess()
    {
        IApplicationDbContext context = Substitute.For<IApplicationDbContext>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        DbSet<Tree> treeDbSet = Substitute.For<DbSet<Tree>>();
        DbSet<TreeRbac> rbacDbSet = Substitute.For<DbSet<TreeRbac>>();

        context.Trees.Returns(treeDbSet);
        context.TreeRbacs.Returns(rbacDbSet);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

        var handler = new CreateTreeCommandHandler(context, unitOfWork);
        var command = new CreateTreeCommand
        {
            Name = "Family Tree",
            Description = "A family tree",
            IsPublic = true,
            OwnerId = Guid.NewGuid()
        };

        Result<Guid> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }
    [Fact]
    public async Task HandleAsync_WithInvalidName_ReturnsFailure()
    {
        IApplicationDbContext context = Substitute.For<IApplicationDbContext>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        DbSet<Tree> treeDbSet = Substitute.For<DbSet<Tree>>();
        DbSet<TreeRbac> rbacDbSet = Substitute.For<DbSet<TreeRbac>>();

        context.Trees.Returns(treeDbSet);
        context.TreeRbacs.Returns(rbacDbSet);

        var handler = new CreateTreeCommandHandler(context, unitOfWork);
        var command = new CreateTreeCommand
        {
            Name = "   ",
            Description = "A family tree",
            IsPublic = true,
            OwnerId = Guid.NewGuid()
        };

        Result<Guid> result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.TreeErrors.InvalidTreeName);
    }
}
