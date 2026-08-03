using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.ValueObjects;

namespace FamilyTreeApp.Application.Roster.DTOs;

public record FamilyMemberDto
{
    public required Guid FamilyMemberId { get; init; }
    public required Guid TreeId { get; init; }
    public Guid? ClaimedByUserId { get; init; }
    public required ProfileInfo ProfileInfo { get; init; }
    public required VisibilityStatus VisibilityStatus { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
