using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReUse.Domain.Entities;
using ReUse.Domain.Enums;

namespace ReUse.Infrastructure.Persistence.Configurations;

public class NotificationDeliveryEntityTypeConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("notification_deliveries");

        // PK
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
               .ValueGeneratedNever();

        // Properties
        builder.Property(d => d.Channel)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(d => d.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired()
               .HasDefaultValue(DeliveryStatus.Pending);

        builder.Property(d => d.RetryCount)
               .HasDefaultValue(0)
               .IsRequired();

        builder.Property(d => d.ErrorMessage)
               .HasMaxLength(1000);

        // Audit
        builder.Property(d => d.CreatedAt)
               .IsRequired();

        // FK → Notification
        builder.Property(d => d.NotificationId)
               .IsRequired();

        // Indexes
        builder.HasIndex(d => new { d.NotificationId, d.Channel });
        builder.HasIndex(d => new { d.Status, d.NextRetryAt });
    }
}