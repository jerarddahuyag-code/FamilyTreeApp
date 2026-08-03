using FamilyTreeApp.Domain.Trees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTreeApp.Infrastructure.Persistence.Model_Configurations;

public class TreeConfiguration : IEntityTypeConfiguration<Tree>
{
    public void Configure(EntityTypeBuilder<Tree> builder)
    {
        builder.ToTable("trees_tree");

        builder.HasKey(t => t.TreeId);
        builder.Property(t => t.TreeId).HasColumnName("tree_id");
        builder.Property(t => t.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
        builder.Property(t => t.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(t => t.IsPublic).HasColumnName("is_public").IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(t => t.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(t => t.Name);

        builder.HasMany(t => t.TreeRbacs)
            .WithOne(tr => tr.Tree)
            .HasForeignKey(tr => tr.TreeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
