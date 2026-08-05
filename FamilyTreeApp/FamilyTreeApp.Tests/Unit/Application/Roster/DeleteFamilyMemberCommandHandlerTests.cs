using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Roster.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Common.ValueObjects;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Roster;

public class DeleteFamilyMemberCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly DeleteFamilyMemberCommandHandler _handler;

    public DeleteFamilyMemberCommandHandlerTests()
    {
        _handler = new DeleteFamilyMemberCommandHandler(_dbContextMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccessAndRemovesMember()
    {
        var treeId = Guid.NewGuid();
        FamilyMember member = CreateMember(treeId);

        DbSet<FamilyMember> dbSet = new List<FamilyMember> { member }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(dbSet);

        var command = new DeleteFamilyMemberCommand
        {
            TreeId = treeId,
            FamilyMemberId = member.FamilyMemberId
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _dbContextMock.FamilyMembers.Received(1).Remove(member);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_MemberNotFound_ReturnsFamilyMemberNotFoundError()
    {
        DbSet<FamilyMember> dbSet = new List<FamilyMember>().BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(dbSet);

        var command = new DeleteFamilyMemberCommand
        {
            TreeId = Guid.NewGuid(),
            FamilyMemberId = Guid.NewGuid()
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
    }

    [Fact]
    public async Task HandleAsync_TreeIdMismatch_ReturnsFamilyMemberNotFoundError()
    {
        var memberTreeId = Guid.NewGuid();
        var commandTreeId = Guid.NewGuid();
        FamilyMember member = CreateMember(memberTreeId);

        DbSet<FamilyMember> dbSet = new List<FamilyMember> { member }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(dbSet);

        var command = new DeleteFamilyMemberCommand
        {
            TreeId = commandTreeId,
            FamilyMemberId = member.FamilyMemberId
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
    }

    private static FamilyMember CreateMember(Guid treeId)
    {
        return FamilyMember.Create(
            Guid.NewGuid(),
            treeId,
            null,
            VisibilityStatus.Hidden,
            new ProfileInfo { FirstName = "John", LastName = "Doe" }).Value;
    }
}
