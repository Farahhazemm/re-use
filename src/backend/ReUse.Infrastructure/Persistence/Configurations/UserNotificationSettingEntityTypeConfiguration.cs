using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReUse.Domain.Entities;

namespace ReUse.Infrastructure.Persistence.Configurations;

public class UserNotificationSettingEntityTypeConfiguration : IEntityTypeConfiguration<UserNotificationSetting>
{
    public void Configure(EntityTypeBuilder<UserNotificationSetting> builder)
    {
        builder.ToTable("user_notification_settings");

        // PK
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
               .ValueGeneratedNever();

        // Properties
        builder.Property(s => s.NotificationType)
               .HasConversion<string>()
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(s => s.Channel)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(s => s.IsEnabled)
               .HasDefaultValue(true)
               .IsRequired();

        // Audit
        builder.Property(s => s.CreatedAt)
               .IsRequired();

        // FK → User
        builder.HasOne(s => s.User)
               .WithMany()
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Unique Index
        builder.HasIndex(s => new { s.UserId, s.NotificationType, s.Channel })
               .IsUnique();
    }
}