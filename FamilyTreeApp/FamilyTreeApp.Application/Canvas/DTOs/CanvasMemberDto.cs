using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.ValueObjects;

namespace FamilyTreeApp.Application.Canvas.DTOs;

public record CanvasMemberDto(
    Guid Id,
    ProfileInfo ProfileInfo,
    bool IsMasked,
    VisibilityStatus VisibilityStatus)
{
    public CanvasMemberDto() : this(Guid.Empty, null!, false, VisibilityStatus.Hidden) { }
}
