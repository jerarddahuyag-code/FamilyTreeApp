using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Roster.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Common.ValueObjects;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FluentAssertions;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Roster;

public class AddFamilyMemberCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContextMock = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly AddFamilyMemberCommandHandler _handler;

    public AddFamilyMemberCommandHandlerTests()
    {
        _handler = new AddFamilyMemberCommandHandler(_dbContextMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsFamilyMemberId()
    {
        var command = new AddFamilyMemberCommand
        {
            TreeId = Guid.NewGuid(),
            ProfileInfo = new ProfileInfo { FirstName = "Bob", LastName = "Smith" },
            VisibilityStatus = VisibilityStatus.Hidden
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _dbContextMock.FamilyMembers.Received(1).Add(Arg.Any<FamilyMember>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithNullProfile_ReturnsFailure()
    {
        var command = new AddFamilyMemberCommand
        {
            TreeId = Guid.NewGuid(),
            ProfileInfo = null!,
            VisibilityStatus = VisibilityStatus.Hidden
        };

        Result<Guid> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.InvalidProfile);
    }
}
