using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Trees.Enums;

namespace FamilyTreeApp.Domain.Canvas.Services;

/// <summary>
/// Domain service that enforces member visibility privacy rules on canvas data.
/// Hidden members are masked as Anonymous for non-admin requesters.
/// </summary>
public interface IVisibilityService
{
    /// <summary>
    /// Resolves visibility for all family members contained within canvas nodes.
    /// Use this when projecting a full canvas (TreeNode graph) for a requesting user.
    /// </summary>
    Dictionary<Guid, CanvasMemberVisibility> ResolveForCanvas(
        IEnumerable<TreeNode> nodes,
        TreeRole? requesterRole);

    /// <summary>
    /// Resolves visibility for a flat list of family members.
    /// Use this when working with members directly (e.g., roster-level projection).
    /// </summary>
    Dictionary<Guid, CanvasMemberVisibility> ResolveForMembers(
        IEnumerable<FamilyMember> members,
        TreeRole? requesterRole);
}
