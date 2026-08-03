using FamilyTreeApp.Domain.Roster.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTreeApp.Infrastructure.Persistence.Model_Configurations;

public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.ToTable("family_members");
        builder.HasKey(t => t.FamilyMemberId);
        builder.Property(t => t.FamilyMemberId).HasColumnName("family_member_id");
        builder.Property(t => t.TreeId).HasColumnName("tree_id").IsRequired();
        builder.Property(t => t.ClaimedByUserId).HasColumnName("claimed_by_user_id");

        builder.OwnsOne(u => u.ProfileInfo, p =>
        {
            p.Property(pi => pi.FirstName).HasColumnName("first_name").HasMaxLength(100);
            p.Property(pi => pi.LastName).HasColumnName("last_name").HasMaxLength(100);
            p.Property(pi => pi.BirthDate).HasColumnName("date_of_birth");
            p.Property(pi => pi.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(500);
            p.Property(pi => pi.PhoneNumber).HasColumnName("phone_number").HasMaxLength(50);
            p.Property(pi => pi.Gender).HasColumnName("gender").HasMaxLength(50);
            p.Property(pi => pi.Bio).HasColumnName("bio").HasMaxLength(500);
        });

        builder.Property(t => t.VisibilityStatus).HasColumnName("visibility_status").IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasMany(t => t.Relationships)
               .WithOne()
               .HasForeignKey(r => r.BaseFamilyMemberId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
