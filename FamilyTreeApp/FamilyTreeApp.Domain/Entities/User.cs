using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.ValueObjects;

namespace FamilyTreeApp.Domain.Entities;

public class User : AggregateRoot
{
    public required Guid UserId { get; set; }

    public required string Email { get; set; }

    public required bool IsPublic { get; set; }

    public required ProfileInfo ProfileInfo { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
