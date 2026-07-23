using FamilyTreeApp.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTreeApp.Infrastructure.Persistence.Model_Configurations;

public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("users_external_logins");

        builder.HasKey(t => t.ExternalLoginId);
        builder.Property(t => t.ExternalLoginId).HasColumnName("external_login_id");
        builder.Property(t => t.UserId).HasColumnName("user_id");
        builder.Property(t => t.Provider).HasColumnName("provider").IsRequired().HasMaxLength(100);
        builder.Property(t => t.ProviderKey).HasColumnName("provider_key").IsRequired().HasMaxLength(255);
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(t => new { t.Provider, t.ProviderKey }).IsUnique();
    }
}
