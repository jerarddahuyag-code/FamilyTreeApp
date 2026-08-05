using FamilyTreeApp.Domain.Roster.Entities;

namespace FamilyTreeApp.Domain.Canvas.Entities;

public class TreeNodeMember
{
    public Guid TreeNodeId { get; private set; }
    public TreeNode TreeNode { get; private set; } = null!;

    public Guid FamilyMemberId { get; private set; }
    public FamilyMember FamilyMember { get; private set; } = null!;

    private TreeNodeMember() { }

    public TreeNodeMember(Guid treeNodeId, Guid familyMemberId)
    {
        TreeNodeId = treeNodeId;
        FamilyMemberId = familyMemberId;
    }
}
