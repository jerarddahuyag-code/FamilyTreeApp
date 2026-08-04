using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Roster.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Roster;

public class RemoveRelationshipCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly RemoveRelationshipCommandHandler _handler;

    public RemoveRelationshipCommandHandlerTests()
    {
        _handler = new RemoveRelationshipCommandHandler(_dbContextMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccess()
    {
        var treeId = Guid.NewGuid();
        FamilyMemberRelationship relationship = FamilyMemberRelationship.Create(
            Guid.NewGuid(),
            treeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            RelationshipType.Parent).Value;

        DbSet<FamilyMemberRelationship> dbSet = new List<FamilyMemberRelationship> { relationship }.BuildMockDbSet();
        _dbContextMock.FamilyMemberRelationships.Returns(dbSet);

        var command = new RemoveRelationshipCommand
        {
            TreeId = treeId,
            FamilyMemberRelationshipId = relationship.FamilyMemberRelationshipId
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _dbContextMock.FamilyMemberRelationships.Received(1).Remove(relationship);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_RelationshipNotFound_ReturnsRelationshipNotFoundError()
    {
        DbSet<FamilyMemberRelationship> dbSet = new List<FamilyMemberRelationship>().BuildMockDbSet();
        _dbContextMock.FamilyMemberRelationships.Returns(dbSet);

        var command = new RemoveRelationshipCommand
        {
            TreeId = Guid.NewGuid(),
            FamilyMemberRelationshipId = Guid.NewGuid()
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberRelationshipErrors.RelationshipNotFound);
    }

    [Fact]
    public async Task HandleAsync_TreeIdMismatch_ReturnsRelationshipNotFoundError()
    {
        var relationshipTreeId = Guid.NewGuid();
        var commandTreeId = Guid.NewGuid();

        FamilyMemberRelationship relationship = FamilyMemberRelationship.Create(
            Guid.NewGuid(),
            relationshipTreeId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            RelationshipType.Spouse).Value;

        DbSet<FamilyMemberRelationship> dbSet = new List<FamilyMemberRelationship> { relationship }.BuildMockDbSet();
        _dbContextMock.FamilyMemberRelationships.Returns(dbSet);

        var command = new RemoveRelationshipCommand
        {
            TreeId = commandTreeId,
            FamilyMemberRelationshipId = relationship.FamilyMemberRelationshipId
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberRelationshipErrors.RelationshipNotFound);
    }
}
