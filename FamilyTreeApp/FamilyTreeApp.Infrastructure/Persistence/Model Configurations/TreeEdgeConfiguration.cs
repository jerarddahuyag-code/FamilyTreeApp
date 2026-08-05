using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Trees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTreeApp.Infrastructure.Persistence.Model_Configurations;

public class TreeEdgeConfiguration : IEntityTypeConfiguration<TreeEdge>
{
    public void Configure(EntityTypeBuilder<TreeEdge> builder)
    {
        builder.ToTable("canvas_treeedge");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.TreeId).HasColumnName("tree_id").IsRequired();
        builder.Property(e => e.SourceNodeId).HasColumnName("source_node_id").IsRequired();
        builder.Property(e => e.TargetNodeId).HasColumnName("target_node_id").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(e => e.TreeId);

        builder.HasOne<Tree>()
            .WithMany()
            .HasForeignKey(e => e.TreeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<TreeNode>()
            .WithMany()
            .HasForeignKey(e => e.SourceNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<TreeNode>()
            .WithMany()
            .HasForeignKey(e => e.TargetNodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
