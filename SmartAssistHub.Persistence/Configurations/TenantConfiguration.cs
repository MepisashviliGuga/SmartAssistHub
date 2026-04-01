using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAssistHub.Domain.Entities;
using SmartAssistHub.Domain.ValueObjects;

namespace SmartAssistHub.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(
                slug => slug.Value,
                value => Slug.FromTrustedValue(value));

        builder.HasIndex(t => t.Slug)
            .IsUnique()
            .HasDatabaseName("UX_Tenants_Slug");

        builder.Property(t => t.IsActive)
            .IsRequired();

        builder.Property(t => t.Plan)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.MonthlyTokenLimit)
            .IsRequired();

        builder.Property(t => t.TokensUsedThisMonth)
            .IsRequired();

        builder.Property(t => t.RowVersion)
            .IsRowVersion();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();

        builder.HasMany(t => t.Users)
            .WithOne()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Documents)
            .WithOne()
            .HasForeignKey(d => d.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}