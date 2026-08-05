using FamilyTreeApp.Domain.Canvas.ValueObjects;
using FluentAssertions;

namespace FamilyTreeApp.Tests.Unit.Domain;

public class CanvasCoordinatesTests
{
    [Fact]
    public void Constructor_WithValidCoordinates_SetsXAndY()
    {
        // Arrange & Act
        var coordinates = new CanvasCoordinates(120.5, 340.75);

        // Assert
        coordinates.X.Should().Be(120.5);
        coordinates.Y.Should().Be(340.75);
    }

    [Fact]
    public void Equals_WithSameCoordinates_ReturnsTrue()
    {
        // Arrange
        var coord1 = new CanvasCoordinates(100.0, 200.0);
        var coord2 = new CanvasCoordinates(100.0, 200.0);

        // Act & Assert
        coord1.Should().Be(coord2);
        (coord1 == coord2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCoordinates_ReturnsFalse()
    {
        // Arrange
        var coord1 = new CanvasCoordinates(100.0, 200.0);
        var coord2 = new CanvasCoordinates(100.0, 205.0);

        // Act & Assert
        coord1.Should().NotBe(coord2);
        (coord1 == coord2).Should().BeFalse();
    }
}
