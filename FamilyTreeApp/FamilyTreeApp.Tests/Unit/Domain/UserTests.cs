using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Common.ValueObjects;
using FamilyTreeApp.Domain.Users.Entities;
using FluentAssertions;

namespace FamilyTreeApp.Tests.Unit.Domain;

public class UserTests
{
    [Fact]
    public void Create_WithValidEmailAndProfile_ReturnsSuccess()
    {
        Result<User> result = User.Create(Guid.NewGuid(), "person@example.com", CreateProfile());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyEmail_ReturnsFailure()
    {
        Result<User> result = User.Create(Guid.NewGuid(), string.Empty, CreateProfile());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.UserErrors.InvalidEmail);
    }

    [Fact]
    public void Create_WithWhitespaceEmail_ReturnsFailure()
    {
        Result<User> result = User.Create(Guid.NewGuid(), "   ", CreateProfile());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.UserErrors.InvalidEmail);
    }

    [Fact]
    public void Create_WithMalformedEmail_ReturnsFailure()
    {
        Result<User> result = User.Create(Guid.NewGuid(), "notanemail", CreateProfile());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.UserErrors.InvalidEmail);
    }

    [Fact]
    public void Create_WithNullProfile_ReturnsFailure()
    {
        Result<User> result = User.Create(Guid.NewGuid(), "person@example.com", null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.UserErrors.InvalidProfile);
    }

    private static ProfileInfo CreateProfile()
    {
        return new ProfileInfo
        {
            FirstName = "Ada",
            LastName = "Lovelace"
        };
    }
}
