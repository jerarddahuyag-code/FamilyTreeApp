using FamilyTreeApp.Domain.Canvas.Entities;
using FamilyTreeApp.Domain.Trees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTreeApp.Infrastructure.Persistence.Model_Configurations;

public class TreeNodeConfiguration : IEntityTypeConfiguration<TreeNode>
{
    public void Configure(EntityTypeBuilder<TreeNode> builder)
    {
        builder.ToTable("canvas_treenode");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.TreeId).HasColumnName("tree_id").IsRequired();
        builder.Property(n => n.NodeType).HasColumnName("node_type").IsRequired();
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(n => n.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.OwnsOne(n => n.Coordinates, c =>
        {
            c.Property(p => p.X).HasColumnName("x").IsRequired();
            c.Property(p => p.Y).HasColumnName("y").IsRequired();
        });

        builder.HasIndex(n => n.TreeId);

        builder.HasOne<Tree>()
            .WithMany()
            .HasForeignKey(n => n.TreeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(n => n.Members)
            .WithOne(m => m.TreeNode)
            .HasForeignKey(m => m.TreeNodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
