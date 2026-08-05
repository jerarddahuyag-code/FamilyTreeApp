using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.Trees.Enums;
using FamilyTreeApp.Domain.ValueObjects;

namespace FamilyTreeApp.Domain.Canvas.Services;

public record CanvasMemberVisibility(
    Guid FamilyMemberId,
    ProfileInfo ProfileInfo,
    bool IsMasked,
    VisibilityStatus VisibilityStatus);

public class VisibilityMediator : IVisibilityMediator
{
    public static Dictionary<Guid, CanvasMemberVisibility> ResolveVisibility(
        IEnumerable<TreeNode> nodes,
        TreeRole? requesterRole)
    {
        var isTreeAdmin = requesterRole is TreeRole.Owner or TreeRole.Admin;
        return ResolveVisibility(nodes, isTreeAdmin);
    }

    public static Dictionary<Guid, CanvasMemberVisibility> ResolveVisibility(
        IEnumerable<TreeNode> nodes,
        bool isTreeAdmin)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        IEnumerable<FamilyMember> familyMembers = nodes
            .SelectMany(n => n.Members)
            .Select(m => m.FamilyMember)
            .Where(m => m != null!);

        return ResolveVisibility(familyMembers!, isTreeAdmin);
    }

    public static Dictionary<Guid, CanvasMemberVisibility> ResolveVisibility(
        IEnumerable<FamilyMember> members,
        TreeRole? requesterRole)
    {
        var isTreeAdmin = requesterRole is TreeRole.Owner or TreeRole.Admin;
        return ResolveVisibility(members, isTreeAdmin);
    }

    public static Dictionary<Guid, CanvasMemberVisibility> ResolveVisibility(
        IEnumerable<FamilyMember> members,
        bool isTreeAdmin)
    {
        ArgumentNullException.ThrowIfNull(members);

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

    Dictionary<Guid, CanvasMemberVisibility> IVisibilityMediator.ResolveVisibility(
        IEnumerable<TreeNode> nodes,
        TreeRole? requesterRole) => ResolveVisibility(nodes, requesterRole);

    Dictionary<Guid, CanvasMemberVisibility> IVisibilityMediator.ResolveVisibility(
        IEnumerable<TreeNode> nodes,
        bool isTreeAdmin) => ResolveVisibility(nodes, isTreeAdmin);

    Dictionary<Guid, CanvasMemberVisibility> IVisibilityMediator.ResolveVisibility(
        IEnumerable<FamilyMember> members,
        TreeRole? requesterRole) => ResolveVisibility(members, requesterRole);

    Dictionary<Guid, CanvasMemberVisibility> IVisibilityMediator.ResolveVisibility(
        IEnumerable<FamilyMember> members,
        bool isTreeAdmin) => ResolveVisibility(members, isTreeAdmin);

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
