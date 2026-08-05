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

public class UpdateFamilyMemberCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly UpdateFamilyMemberProfileCommandHandler _handler;

    public UpdateFamilyMemberCommandHandlerTests()
    {
        _handler = new UpdateFamilyMemberProfileCommandHandler(_dbContextMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccessAndUpdatesMember()
    {
        var treeId = Guid.NewGuid();
        FamilyMember member = CreateMember(treeId);

        DbSet<FamilyMember> dbSet = new List<FamilyMember> { member }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(dbSet);

        var updatedProfile = new ProfileInfo { FirstName = "UpdatedFirst", LastName = "UpdatedLast" };
        var command = new UpdateFamilyMemberProfileCommand
        {
            TreeId = treeId,
            FamilyMemberId = member.FamilyMemberId,
            ProfileInfo = updatedProfile
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _dbContextMock.FamilyMembers.Received(1).Update(Arg.Is<FamilyMember>(m =>
            m != null &&
            m.FamilyMemberId == member.FamilyMemberId &&
            m.ProfileInfo!.FirstName == "UpdatedFirst" &&
            m.ProfileInfo!.LastName == "UpdatedLast"));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_MemberNotFound_ReturnsFamilyMemberNotFoundError()
    {
        DbSet<FamilyMember> dbSet = new List<FamilyMember>().BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(dbSet);

        var command = new UpdateFamilyMemberProfileCommand
        {
            TreeId = Guid.NewGuid(),
            FamilyMemberId = Guid.NewGuid(),
            ProfileInfo = new ProfileInfo { FirstName = "Test", LastName = "User" }
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

        var command = new UpdateFamilyMemberProfileCommand
        {
            TreeId = commandTreeId,
            FamilyMemberId = member.FamilyMemberId,
            ProfileInfo = new ProfileInfo { FirstName = "Test", LastName = "User" }
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
    }

    [Fact]
    public async Task HandleAsync_InvalidProfile_ReturnsInvalidProfileError()
    {
        var treeId = Guid.NewGuid();
        FamilyMember member = CreateMember(treeId);

        DbSet<FamilyMember> dbSet = new List<FamilyMember> { member }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(dbSet);

        var command = new UpdateFamilyMemberProfileCommand
        {
            TreeId = treeId,
            FamilyMemberId = member.FamilyMemberId,
            ProfileInfo = null!
        };

        Result<bool> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.InvalidProfile);
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
