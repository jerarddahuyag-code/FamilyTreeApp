using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Roster.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Roster;

public class RequestVisibilityCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly RequestVisibilityCommandHandler _handler;

    public RequestVisibilityCommandHandlerTests()
    {
        _handler = new RequestVisibilityCommandHandler(_dbContextMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccessAndUpdatesVisibility()
    {
        var treeId = Guid.NewGuid();
        FamilyMember member = CreateMember(treeId, VisibilityStatus.Hidden);

        DbSet<FamilyMember> dbSet = new List<FamilyMember> { member }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(dbSet);

        var command = new RequestVisibilityCommand
        {
            TreeId = treeId,
            FamilyMemberId = member.FamilyMemberId,
            TargetVisibilityStatus = VisibilityStatus.Pending
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        member.VisibilityStatus.Should().Be(VisibilityStatus.Pending);
        _dbContextMock.FamilyMembers.Received(1).Update(member);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_MemberNotFound_ReturnsFamilyMemberNotFoundError()
    {
        DbSet<FamilyMember> dbSet = new List<FamilyMember>().BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(dbSet);

        var command = new RequestVisibilityCommand
        {
            TreeId = Guid.NewGuid(),
            FamilyMemberId = Guid.NewGuid(),
            TargetVisibilityStatus = VisibilityStatus.Pending
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
        FamilyMember member = CreateMember(memberTreeId, VisibilityStatus.Hidden);

        DbSet<FamilyMember> dbSet = new List<FamilyMember> { member }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(dbSet);

        var command = new RequestVisibilityCommand
        {
            TreeId = commandTreeId,
            FamilyMemberId = member.FamilyMemberId,
            TargetVisibilityStatus = VisibilityStatus.Pending
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
    }

    [Fact]
    public async Task HandleAsync_InvalidTransition_ReturnsFailure()
    {
        var treeId = Guid.NewGuid();
        FamilyMember member = CreateMember(treeId, VisibilityStatus.Hidden);

        DbSet<FamilyMember> dbSet = new List<FamilyMember> { member }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(dbSet);

        var command = new RequestVisibilityCommand
        {
            TreeId = treeId,
            FamilyMemberId = member.FamilyMemberId,
            TargetVisibilityStatus = VisibilityStatus.Visible
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.InvalidVisibilityTransition);
    }

    private static FamilyMember CreateMember(Guid treeId, VisibilityStatus visibilityStatus)
    {
        return FamilyMember.Create(
            Guid.NewGuid(),
            treeId,
            Guid.NewGuid(),
            visibilityStatus,
            new ProfileInfo { FirstName = "John", LastName = "Doe" }).Value;
    }
}
