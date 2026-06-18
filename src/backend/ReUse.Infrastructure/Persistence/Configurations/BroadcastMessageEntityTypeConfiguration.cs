using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReUse.Domain.Entities;
using ReUse.Domain.Enums;

namespace ReUse.Infrastructure.Persistence.Configurations;

public class BroadcastMessageEntityTypeConfiguration : IEntityTypeConfiguration<BroadcastMessage>
{
    public void Configure(EntityTypeBuilder<BroadcastMessage> builder)
    {
        builder.ToTable("broadcast_messages");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();

        builder.Property(b => b.Title)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(b => b.Body)
               .HasMaxLength(1000)
               .IsRequired();

        builder.Property(b => b.TargetAudience)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(b => b.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired()
               .HasDefaultValue(BroadcastStatus.Draft);

        builder.Property(b => b.RecipientCount).HasDefaultValue(0);
        builder.Property(b => b.DeliveredCount).HasDefaultValue(0);
        builder.Property(b => b.FailedCount).HasDefaultValue(0);

        builder.Property(b => b.CreatedAt).IsRequired();

        builder.Property(b => b.RowVersion)
               .HasColumnName("xmin")
               .HasColumnType("xid")
               .ValueGeneratedOnAddOrUpdate()
               .IsConcurrencyToken();

        builder.HasOne(b => b.CreatedBy)
               .WithMany()
               .HasForeignKey(b => b.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => new { b.Status, b.ScheduledAt });
        builder.HasIndex(b => b.CreatedAt);
    }
}