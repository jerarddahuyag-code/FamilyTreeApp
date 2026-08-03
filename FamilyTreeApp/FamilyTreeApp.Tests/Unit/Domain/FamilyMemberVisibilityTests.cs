using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyTreeApp.Tests.Unit.Domain;

public class FamilyMemberVisibilityTests
{
    [Fact]
    public void TransitionToVisbility_FromHiddenToPending_ReturnsSuccess()
    {
        FamilyMember member = FamilyMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            VisibilityStatus.Hidden,
            CreateProfile()).Value;

        Result result = member.TransitionToVisbility(VisibilityStatus.Pending);

        result.IsSuccess.Should().BeTrue();
        member.VisibilityStatus.Should().Be(VisibilityStatus.Pending);
    }

    [Fact]
    public void TransitionToVisbility_FromPendingToVisible_ReturnsSuccess()
    {
        FamilyMember member = FamilyMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            VisibilityStatus.Hidden,
            CreateProfile()).Value;

        member.TransitionToVisbility(VisibilityStatus.Pending);
        Result result = member.TransitionToVisbility(VisibilityStatus.Visible);

        result.IsSuccess.Should().BeTrue();
        member.VisibilityStatus.Should().Be(VisibilityStatus.Visible);
    }

    [Fact]
    public void TransitionToVisbility_FromVisibleToPending_ReturnsFailure()
    {
        FamilyMember member = FamilyMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            VisibilityStatus.Hidden,
            CreateProfile()).Value;

        member.TransitionToVisbility(VisibilityStatus.Pending);
        member.TransitionToVisbility(VisibilityStatus.Visible);

        Result result = member.TransitionToVisbility(VisibilityStatus.Pending);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.InvalidVisibilityTransition);
    }

    private static ProfileInfo CreateProfile() => new() { FirstName = "Jane", LastName = "Doe" };
}
