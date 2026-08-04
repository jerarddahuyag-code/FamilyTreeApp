using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.ValueObjects;

namespace FamilyTreeApp.Domain.Roster.Entities;

public class FamilyMember : AggregateRoot
{
    public Guid FamilyMemberId { get; private set; }

    public Guid TreeId { get; private set; }

    public Guid? ClaimedByUserId { get; private set; }

    public ProfileInfo ProfileInfo { get; private set; } = null!;

    public VisibilityStatus VisibilityStatus { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public ICollection<FamilyMemberRelationship> Relationships { get; private set; } = [];

    private FamilyMember() { }

    private FamilyMember(Guid familyMemberId, Guid treeId, Guid? claimedByUserId, VisibilityStatus visibilityStatus, ProfileInfo profileInfo)
    {
        FamilyMemberId = familyMemberId;
        TreeId = treeId;
        ClaimedByUserId = claimedByUserId;
        ProfileInfo = profileInfo;
        VisibilityStatus = visibilityStatus;
        CreatedAt = UpdatedAt = DateTime.UtcNow;
    }

    public static Result<FamilyMember> Create(Guid familyMemberId, Guid treeId, Guid? claimedByUserId, VisibilityStatus visibilityStatus, ProfileInfo profileInfo)
    {
        if (profileInfo is null)
        {
            return Result.Failure<FamilyMember>(DomainErrors.FamilyMemberErrors.InvalidProfile);
        }

        if (visibilityStatus == VisibilityStatus.Pending)
        {
            return Result.Failure<FamilyMember>(DomainErrors.FamilyMemberErrors.InvalidVisibilityStatus);
        }

        var familyMember = new FamilyMember(familyMemberId, treeId, claimedByUserId, visibilityStatus, profileInfo);
        return Result.Success(familyMember);
    }

    public Result TransitionToVisibility(VisibilityStatus visibilityStatus)
    {
        if (CanTransitionToVisibility(visibilityStatus))
        {
            VisibilityStatus = visibilityStatus;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        return Result.Failure(DomainErrors.FamilyMemberErrors.InvalidVisibilityTransition);
    }

    private bool CanTransitionToVisibility(VisibilityStatus visibilityStatus)
    {
        if (VisibilityStatus == VisibilityStatus.Pending && visibilityStatus == VisibilityStatus.Visible)
        {
            return true;
        }

        if (VisibilityStatus == VisibilityStatus.Pending && visibilityStatus == VisibilityStatus.Hidden)
        {
            return true;
        }

        if (VisibilityStatus == VisibilityStatus.Visible && visibilityStatus == VisibilityStatus.Hidden)
        {
            return true;
        }

        if (VisibilityStatus == VisibilityStatus.Hidden && visibilityStatus == VisibilityStatus.Pending)
        {
            return true;
        }

        if (ClaimedByUserId == null && VisibilityStatus == VisibilityStatus.Hidden && visibilityStatus == VisibilityStatus.Visible)
        {
            return true;
        }

        return false;
    }
}
