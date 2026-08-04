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

public class AddRelationshipCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly AddRelationshipCommandHandler _handler;

    public AddRelationshipCommandHandlerTests()
    {
        _handler = new AddRelationshipCommandHandler(_dbContextMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsRelationshipId()
    {
        var treeId = Guid.NewGuid();
        FamilyMember baseMember = CreateMember(treeId);
        FamilyMember relatedMember = CreateMember(treeId);

        DbSet<FamilyMember> membersDbSet = new List<FamilyMember> { baseMember, relatedMember }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(membersDbSet);

        DbSet<FamilyMemberRelationship> relationshipsDbSet = new List<FamilyMemberRelationship>().BuildMockDbSet();
        _dbContextMock.FamilyMemberRelationships.Returns(relationshipsDbSet);

        var command = new AddRelationshipCommand
        {
            TreeId = treeId,
            BaseFamilyMemberId = baseMember.FamilyMemberId,
            RelatedFamilyMemberId = relatedMember.FamilyMemberId,
            RelationshipType = RelationshipType.Parent
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _dbContextMock.FamilyMemberRelationships.Received(1).Add(Arg.Any<FamilyMemberRelationship>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_BaseMemberNotFound_ReturnsFamilyMemberNotFoundError()
    {
        var treeId = Guid.NewGuid();
        FamilyMember relatedMember = CreateMember(treeId);

        DbSet<FamilyMember> membersDbSet = new List<FamilyMember> { relatedMember }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(membersDbSet);

        var command = new AddRelationshipCommand
        {
            TreeId = treeId,
            BaseFamilyMemberId = Guid.NewGuid(),
            RelatedFamilyMemberId = relatedMember.FamilyMemberId,
            RelationshipType = RelationshipType.Parent
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
    }

    [Fact]
    public async Task HandleAsync_RelatedMemberNotFound_ReturnsFamilyMemberNotFoundError()
    {
        var treeId = Guid.NewGuid();
        FamilyMember baseMember = CreateMember(treeId);

        DbSet<FamilyMember> membersDbSet = new List<FamilyMember> { baseMember }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(membersDbSet);

        var command = new AddRelationshipCommand
        {
            TreeId = treeId,
            BaseFamilyMemberId = baseMember.FamilyMemberId,
            RelatedFamilyMemberId = Guid.NewGuid(),
            RelationshipType = RelationshipType.Parent
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.FamilyMemberNotFound);
    }

    [Fact]
    public async Task HandleAsync_BaseMemberTreeMismatch_ReturnsMemberTreeMismatchError()
    {
        var treeId = Guid.NewGuid();
        FamilyMember baseMember = CreateMember(Guid.NewGuid());
        FamilyMember relatedMember = CreateMember(treeId);

        DbSet<FamilyMember> membersDbSet = new List<FamilyMember> { baseMember, relatedMember }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(membersDbSet);

        var command = new AddRelationshipCommand
        {
            TreeId = treeId,
            BaseFamilyMemberId = baseMember.FamilyMemberId,
            RelatedFamilyMemberId = relatedMember.FamilyMemberId,
            RelationshipType = RelationshipType.Child
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberRelationshipErrors.MemberTreeMismatch);
    }

    [Fact]
    public async Task HandleAsync_RelatedMemberTreeMismatch_ReturnsMemberTreeMismatchError()
    {
        var treeId = Guid.NewGuid();
        FamilyMember baseMember = CreateMember(treeId);
        FamilyMember relatedMember = CreateMember(Guid.NewGuid());

        DbSet<FamilyMember> membersDbSet = new List<FamilyMember> { baseMember, relatedMember }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(membersDbSet);

        var command = new AddRelationshipCommand
        {
            TreeId = treeId,
            BaseFamilyMemberId = baseMember.FamilyMemberId,
            RelatedFamilyMemberId = relatedMember.FamilyMemberId,
            RelationshipType = RelationshipType.Child
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberRelationshipErrors.MemberTreeMismatch);
    }

    [Fact]
    public async Task HandleAsync_SameBaseAndRelatedMember_ReturnsSameFamilyMembersError()
    {
        var treeId = Guid.NewGuid();
        FamilyMember member = CreateMember(treeId);

        DbSet<FamilyMember> membersDbSet = new List<FamilyMember> { member }.BuildMockDbSet();
        _dbContextMock.FamilyMembers.Returns(membersDbSet);

        var command = new AddRelationshipCommand
        {
            TreeId = treeId,
            BaseFamilyMemberId = member.FamilyMemberId,
            RelatedFamilyMemberId = member.FamilyMemberId,
            RelationshipType = RelationshipType.Spouse
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberRelationshipErrors.SameFamilyMembers);
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
