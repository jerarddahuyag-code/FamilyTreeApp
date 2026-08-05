using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Trees.Enums;

namespace FamilyTreeApp.Domain.Canvas.Services;

public interface IVisibilityMediator
{
    Dictionary<Guid, CanvasMemberVisibility> ResolveVisibility(
        IEnumerable<TreeNode> nodes,
        TreeRole? requesterRole);

    Dictionary<Guid, CanvasMemberVisibility> ResolveVisibility(
        IEnumerable<TreeNode> nodes,
        bool isTreeAdmin);

    Dictionary<Guid, CanvasMemberVisibility> ResolveVisibility(
        IEnumerable<FamilyMember> members,
        TreeRole? requesterRole);

    Dictionary<Guid, CanvasMemberVisibility> ResolveVisibility(
        IEnumerable<FamilyMember> members,
        bool isTreeAdmin);
}
