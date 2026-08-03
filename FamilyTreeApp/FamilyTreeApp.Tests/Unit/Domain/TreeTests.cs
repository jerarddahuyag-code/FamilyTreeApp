using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Trees.Entities;
using FluentAssertions;

namespace FamilyTreeApp.Tests.Unit.Domain;

public class TreeTests
{
    [Fact]
    public void Create_WithValidName_ReturnsSuccess()
    {
        Result<Tree> result = Tree.Create(Guid.NewGuid(), "Family Tree", "A description", isPublic: false);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsFailure()
    {
        Result<Tree> result = Tree.Create(Guid.NewGuid(), string.Empty, "A description", isPublic: false);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.TreeErrors.InvalidTreeName);
    }

    [Fact]
    public void Create_WithWhitespaceName_ReturnsFailure()
    {
        Result<Tree> result = Tree.Create(Guid.NewGuid(), "   ", "A description", isPublic: false);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.TreeErrors.InvalidTreeName);
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_ReturnsFailure()
    {
        Tree tree = CreateTree();

        Result result = tree.UpdateDetails(string.Empty, "Updated description");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_WithValidName_ReturnsSuccess()
    {
        Tree tree = CreateTree();

        Result result = tree.UpdateDetails("Updated tree", "Updated description");

        result.IsSuccess.Should().BeTrue();
        tree.Name.Should().Be("Updated tree");
        tree.Description.Should().Be("Updated description");
    }

    private static Tree CreateTree()
    {
        Result<Tree> result = Tree.Create(Guid.NewGuid(), "Original tree", "Original description", isPublic: false);
        return result.Value;
    }
}
