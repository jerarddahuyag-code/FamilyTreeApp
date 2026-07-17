using FamilyTreeApp.Domain.Common;

namespace FamilyTreeApp.Domain.Entities;

public class User : AggregateRoot
{
    public required Guid UserId { get; set; }

    public required string Email { get; set; }

    public required bool IsPublic { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
