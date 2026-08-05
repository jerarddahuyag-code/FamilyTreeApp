using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Common.Errors.ValueObjects;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FluentAssertions;

namespace FamilyTreeApp.Tests.Unit.Domain;

public class FamilyMemberTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        Result<FamilyMember> result = FamilyMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            VisibilityStatus.Hidden,
            CreateProfile());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithNullProfile_ReturnsFailure()
    {
        Result<FamilyMember> result = FamilyMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            VisibilityStatus.Hidden,
            null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.InvalidProfile);
    }

    [Fact]
    public void Create_WithPendingVisibilityStatus_ReturnsFailure()
    {
        Result<FamilyMember> result = FamilyMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            VisibilityStatus.Pending,
            CreateProfile());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.FamilyMemberErrors.InvalidVisibilityStatus);
    }

    private static ProfileInfo CreateProfile()
    {
        return new ProfileInfo
        {
            FirstName = "John",
            LastName = "Doe"
        };
    }
}
