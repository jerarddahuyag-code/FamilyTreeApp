using FamilyTreeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTreeApp.Infrastructure.Persistence.Model_Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users_user");

        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId).HasColumnName("user_id");
        builder.Property(u => u.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
        builder.Property(u => u.IsPublic).HasColumnName("is_public").IsRequired();
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(u => u.Email).IsUnique();

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
    }
}
