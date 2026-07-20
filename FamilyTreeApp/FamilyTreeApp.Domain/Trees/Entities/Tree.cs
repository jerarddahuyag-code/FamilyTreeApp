using FamilyTreeApp.Domain.Common;

namespace FamilyTreeApp.Domain.Trees.Entities;

public class Tree : AggregateRoot
{
    public required Guid TreeId { get; set; }

    public required string Name { get; set; }

    public required bool IsPublic { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
