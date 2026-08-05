using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Common.Errors.ValueObjects;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FluentAssertions;

namespace FamilyTreeApp.Tests.Unit.Domain;

public class FamilyMemberVisibilityTests
{
    [Fact]
    public void TransitionToVisibility_FromHiddenToPending_ReturnsSuccess()
    {
        FamilyMember member = FamilyMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            VisibilityStatus.Hidden,
            CreateProfile()).Value;

        Result result = member.TransitionToVisibility(VisibilityStatus.Pending);

        result.IsSuccess.Should().BeTrue();
        member.VisibilityStatus.Should().Be(VisibilityStatus.Pending);
    }

    [Fact]
    public void TransitionToVisibility_FromPendingToVisible_ReturnsSuccess()
    {
        FamilyMember member = FamilyMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            VisibilityStatus.Hidden,
            CreateProfile()).Value;

        member.TransitionToVisibility(VisibilityStatus.Pending);
        Result result = member.TransitionToVisibility(VisibilityStatus.Visible);

        result.IsSuccess.Should().BeTrue();
        member.VisibilityStatus.Should().Be(VisibilityStatus.Visible);
    }

    [Fact]
    public void TransitionToVisibility_FromVisibleToPending_ReturnsFailure()
    {
        FamilyMember member = FamilyMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            VisibilityStatus.Hidden,
            CreateProfile()).Value;

        member.TransitionToVisibility(VisibilityStatus.Pending);
        member.TransitionToVisibility(VisibilityStatus.Visible);

        Result result = member.TransitionToVisibility(VisibilityStatus.Pending);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.InvalidVisibilityTransition);
    }

    private static ProfileInfo CreateProfile() => new() { FirstName = "Jane", LastName = "Doe" };
}
