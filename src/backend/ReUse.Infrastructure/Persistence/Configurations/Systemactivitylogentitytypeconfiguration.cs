using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReUse.Domain.Entities;
using ReUse.Domain.Enums;

namespace ReUse.Infrastructure.Persistence.Configurations;

public class SystemActivityLogEntityTypeConfiguration : IEntityTypeConfiguration<SystemActivityLog>
{
    public void Configure(EntityTypeBuilder<SystemActivityLog> builder)
    {
        builder.ToTable("system_activity_logs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.ActionType)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(64);

        builder.Property(l => l.Category)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(64);

        builder.Property(l => l.Severity)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(32)
               .HasDefaultValue(LogSeverity.Info);

        builder.Property(l => l.Status)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(32)
               .HasDefaultValue(LogStatus.Success);

        builder.Property(l => l.Description)
               .IsRequired()
               .HasMaxLength(2000);

        builder.Property(l => l.EntityType)
               .HasMaxLength(128);

        builder.Property(l => l.EntityId)
               .HasMaxLength(128);

        builder.Property(l => l.IpAddress)
               .HasMaxLength(64);

        builder.Property(l => l.UserAgent)
               .HasMaxLength(512);

        builder.Property(l => l.Metadata);

        builder.Property(l => l.CreatedAt).IsRequired();

        builder.Ignore(l => l.UpdatedAt);

        builder.HasOne(l => l.ActorUser)
               .WithMany()
               .HasForeignKey(l => l.ActorUserId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);

        builder.Property(l => l.ActorName)
               .HasMaxLength(256);

        builder.Property(l => l.ActorEmail)
               .HasMaxLength(256);

        // Indexes 

        builder.HasIndex(l => l.CreatedAt);

        builder.HasIndex(l => l.ActorUserId);

        builder.HasIndex(l => new { l.EntityType, l.EntityId });

        builder.HasIndex(l => l.ActionType);
        builder.HasIndex(l => l.Category);

        builder.HasIndex(l => new { l.Category, l.CreatedAt });

        builder.HasIndex(l => l.Severity);
    }
}