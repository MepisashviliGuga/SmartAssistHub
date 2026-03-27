using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAssistHub.Domain.Entities;
using SmartAssistHub.Domain.ValueObjects;

namespace SmartAssistHub.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        builder.Property(d => d.TenantId)
            .IsRequired();

        builder.Property(d => d.UploadedByUserId)
            .IsRequired();

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(260)
            .HasConversion(
                fileName => fileName.Value,
                value => FileName.FromDatabase(value));

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.ComplexProperty(d => d.FileSizeBytes, b =>
        {
            b.Property(f => f.Bytes)
                .HasColumnName("FileSizeBytes")
                .IsRequired();
        });

        builder.Property(d => d.BlobStoragePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.FailureReason)
            .HasMaxLength(1000);

        builder.Property(d => d.ProcessedAt);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .IsRequired();

        builder.HasIndex(d => d.TenantId)
            .HasDatabaseName("IX_Documents_TenantId");

        builder.HasIndex(d => new { d.TenantId, d.Status })
            .HasDatabaseName("IX_Documents_TenantId_Status");

        builder.HasOne<Tenant>()
            .WithMany(t => t.Documents)
            .HasForeignKey(d => d.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(d => d.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Chunks)
            .WithOne()
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}