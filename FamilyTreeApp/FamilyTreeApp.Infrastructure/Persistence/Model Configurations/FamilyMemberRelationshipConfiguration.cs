using FamilyTreeApp.Domain.Roster.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyTreeApp.Infrastructure.Persistence.Model_Configurations;

public class FamilyMemberRelationshipConfiguration : IEntityTypeConfiguration<FamilyMemberRelationship>
{
    public void Configure(EntityTypeBuilder<FamilyMemberRelationship> builder)
    {
        builder.ToTable("family_member_relationships");
        builder.HasKey(fmr => fmr.FamilyMemberRelationshipId);
        builder.Property(fmr => fmr.FamilyMemberRelationshipId).HasColumnName("family_member_relationship_id");
        builder.Property(fmr => fmr.TreeId).HasColumnName("tree_id").IsRequired();
        builder.Property(fmr => fmr.BaseFamilyMemberId).HasColumnName("base_family_member_id").IsRequired();
        builder.Property(fmr => fmr.RelatedFamilyMemberId).HasColumnName("related_family_member_id").IsRequired();
        builder.Property(fmr => fmr.RelationshipType).HasColumnName("relationship_type").IsRequired();
        builder.Property(fmr => fmr.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(fmr => fmr.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(fmr => new { fmr.BaseFamilyMemberId, fmr.RelatedFamilyMemberId, fmr.RelationshipType }).IsUnique();
        builder.HasIndex(fmr => new { fmr.TreeId, fmr.BaseFamilyMemberId });
    }
}
