using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTreeApp.Infrastructure.Persistence.Model_Configurations;

public class TreeRbacConfiguration : IEntityTypeConfiguration<TreeRbac>
{
    public void Configure(EntityTypeBuilder<TreeRbac> builder)
    {
        builder.ToTable("trees_tree_rbac");

        builder.HasKey(t => t.TreeRbacId);
        builder.Property(t => t.TreeRbacId).HasColumnName("tree_rbac_id");
        builder.Property(t => t.TreeId).HasColumnName("tree_id");
        builder.Property(t => t.UserId).HasColumnName("user_id");
        builder.Property(t => t.TreeRole).HasColumnName("tree_role");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(t => new { t.TreeId, t.UserId }).IsUnique();
    }
}
