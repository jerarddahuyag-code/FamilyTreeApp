using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Common.ValueObjects;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.Trees.Enums;

namespace FamilyTreeApp.Domain.Canvas.Services;

public record CanvasMemberVisibility(
    Guid FamilyMemberId,
    ProfileInfo ProfileInfo,
    bool IsMasked,
    VisibilityStatus VisibilityStatus);

/// <summary>
/// Enforces member privacy rules by masking hidden member profiles for non-admin requesters.
/// A member is masked when their VisibilityStatus != Visible and the requester is not Owner/Admin.
/// </summary>
public class VisibilityService : IVisibilityService
{
    /// <inheritdoc/>
    public Dictionary<Guid, CanvasMemberVisibility> ResolveForCanvas(
        IEnumerable<TreeNode> nodes,
        TreeRole? requesterRole)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        IEnumerable<FamilyMember> familyMembers = nodes
            .SelectMany(n => n.Members)
            .Select(m => m.FamilyMember)
            .Where(m => m != null!);

        return ResolveForMembers(familyMembers!, requesterRole);
    }

    /// <inheritdoc/>
    public Dictionary<Guid, CanvasMemberVisibility> ResolveForMembers(
        IEnumerable<FamilyMember> members,
        TreeRole? requesterRole)
    {
        ArgumentNullException.ThrowIfNull(members);

        var isTreeAdmin = requesterRole is TreeRole.Owner or TreeRole.Admin;
        var resultMap = new Dictionary<Guid, CanvasMemberVisibility>();

        foreach (FamilyMember member in members)
        {
            if (resultMap.ContainsKey(member.FamilyMemberId))
            {
                continue;
            }

            var isMasked = member.VisibilityStatus != VisibilityStatus.Visible && !isTreeAdmin;
            ProfileInfo baseProfile = member.ClaimedByUser != null
                ? MergeProfile(member.ClaimedByUser.ProfileInfo, member.ProfileInfo)
                : member.ProfileInfo;

            ProfileInfo effectiveProfile = isMasked
                ? ProfileInfo.CreateAnonymous()
                : baseProfile;

            resultMap[member.FamilyMemberId] = new CanvasMemberVisibility(
                member.FamilyMemberId,
                effectiveProfile,
                isMasked,
                member.VisibilityStatus);
        }

        return resultMap;
    }

    private static ProfileInfo MergeProfile(ProfileInfo userProfile, ProfileInfo memberProfile)
    {
        if (userProfile == null)
        {
            return memberProfile;
        }

        if (memberProfile == null)
        {
            return userProfile;
        }

        return new ProfileInfo
        {
            FirstName = !string.IsNullOrWhiteSpace(userProfile.FirstName) ? userProfile.FirstName : memberProfile.FirstName,
            LastName = !string.IsNullOrWhiteSpace(userProfile.LastName) ? userProfile.LastName : memberProfile.LastName,
            Gender = userProfile.Gender.HasValue ? userProfile.Gender : memberProfile.Gender,
            BirthDate = userProfile.BirthDate ?? memberProfile.BirthDate,
            Bio = userProfile.Bio ?? memberProfile.Bio,
            AvatarUrl = userProfile.AvatarUrl ?? memberProfile.AvatarUrl,
            PhoneNumber = userProfile.PhoneNumber ?? memberProfile.PhoneNumber
        };
    }
}
