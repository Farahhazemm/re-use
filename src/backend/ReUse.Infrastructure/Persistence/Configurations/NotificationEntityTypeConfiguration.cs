using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReUse.Domain.Entities;
using ReUse.Domain.Enums;

namespace ReUse.Infrastructure.Persistence.Configurations;

public class NotificationEntityTypeConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        // PK
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id)
               .ValueGeneratedNever();

        // Properties
        builder.Property(n => n.Title)
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(n => n.Body)
               .IsRequired();

        builder.Property(n => n.Type)
               .HasConversion<string>()
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(n => n.IsRead)
               .HasDefaultValue(false)
               .IsRequired();

        // Dictionary → JSON
        builder.Property(n => n.Data)
               .HasColumnType("jsonb");

        builder.Property(n => n.Metadata)
               .HasColumnType("jsonb");

        // Audit
        builder.Property(n => n.CreatedAt)
               .IsRequired();

        // FK → User
        builder.HasOne(n => n.User)
               .WithMany()
               .HasForeignKey(n => n.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Relationship → NotificationDelivery
        builder.HasMany(n => n.Deliveries)
               .WithOne(d => d.Notification)
               .HasForeignKey(d => d.NotificationId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(n => new { n.UserId, n.CreatedAt });
        builder.HasIndex(n => n.CorrelationId);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
    }
}