using FamilyTreeApp.Domain.Canvas.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTreeApp.Infrastructure.Persistence.Model_Configurations;

public class TreeNodeMemberConfiguration : IEntityTypeConfiguration<TreeNodeMember>
{
    public void Configure(EntityTypeBuilder<TreeNodeMember> builder)
    {
        builder.ToTable("canvas_treenode_member");

        builder.HasKey(m => new { m.TreeNodeId, m.FamilyMemberId });

        builder.Property(m => m.TreeNodeId).HasColumnName("tree_node_id");
        builder.Property(m => m.FamilyMemberId).HasColumnName("family_member_id");

        builder.HasOne(m => m.TreeNode)
            .WithMany(n => n.Members)
            .HasForeignKey(m => m.TreeNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.FamilyMember)
            .WithMany()
            .HasForeignKey(m => m.FamilyMemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
