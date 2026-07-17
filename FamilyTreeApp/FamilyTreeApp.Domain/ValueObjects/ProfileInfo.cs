using FamilyTreeApp.Domain.Common.Enums;

namespace FamilyTreeApp.Domain.ValueObjects;

public record ProfileInfo
{
    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public DateTime? BirthDate { get; init; }

    public string? AvatarUrl { get; init; }

    public string? PhoneNumber { get; init; }

    public Gender? Gender { get; init; }

    public string? Bio { get; init; }
}
