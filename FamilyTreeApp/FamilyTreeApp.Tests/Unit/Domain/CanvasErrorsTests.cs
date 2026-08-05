using FamilyTreeApp.Domain.Common.Errors;
using FluentAssertions;

namespace FamilyTreeApp.Tests.Unit.Domain;

public class CanvasErrorsTests
{
    [Fact]
    public void CanvasErrors_MemberNotInTree_ReturnsExpectedErrorDetails()
    {
        // Act
        Error error = DomainErrors.CanvasErrors.MemberNotInTree;

        // Assert
        error.Code.Should().Be("Canvas.MemberNotInTree");
        error.Message.Should().Be("The member's TreeId does not match the target canvas node's TreeId.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CanvasErrors_NodeNotInTree_ReturnsExpectedErrorDetails()
    {
        // Act
        Error error = DomainErrors.CanvasErrors.NodeNotInTree;

        // Assert
        error.Code.Should().Be("Canvas.NodeNotInTree");
        error.Message.Should().Be("The source or target node's TreeId does not match.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CanvasErrors_NodeNotFound_ReturnsExpectedErrorDetails()
    {
        // Act
        Error error = DomainErrors.CanvasErrors.NodeNotFound;

        // Assert
        error.Code.Should().Be("Canvas.NodeNotFound");
        error.Message.Should().Be("The canvas node was not found.");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void CanvasErrors_EdgeNotFound_ReturnsExpectedErrorDetails()
    {
        // Act
        Error error = DomainErrors.CanvasErrors.EdgeNotFound;

        // Assert
        error.Code.Should().Be("Canvas.EdgeNotFound");
        error.Message.Should().Be("The canvas edge was not found.");
        error.Type.Should().Be(ErrorType.NotFound);
    }
}
