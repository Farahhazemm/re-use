using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReUse.Domain.Entities;
using ReUse.Domain.Enums;

namespace ReUse.Infrastructure.Persistence.Configurations;

public class ReportEntityTypeConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
               .ValueGeneratedNever();

        builder.Property(r => r.TargetType)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(r => r.TargetId)
               .IsRequired();

        builder.Property(r => r.Reason)
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(r => r.Notes)
               .HasMaxLength(1000);

        builder.Property(r => r.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired()
               .HasDefaultValue(ReportStatus.Pending);

        builder.Property(r => r.ReviewedAt);

        builder.Property(r => r.ReviewNotes)
               .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt)
               .IsRequired();

        builder.HasOne(r => r.Reporter)
               .WithMany()
               .HasForeignKey(r => r.ReporterUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReviewedBy)
               .WithMany()
               .HasForeignKey(r => r.ReviewedByUserId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => new { r.ReporterUserId, r.TargetType, r.TargetId })
               .IsUnique();

        builder.HasIndex(r => new { r.TargetType, r.TargetId, r.Status });

        builder.HasIndex(r => new { r.Status, r.CreatedAt });

        builder.HasIndex(r => r.ReporterUserId);
    }
}