using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReUse.Domain.Entities;

namespace ReUse.Infrastructure.Persistence.Configurations;

public class ActivityEventEntityTypeConfiguration : IEntityTypeConfiguration<ActivityEvent>
{
    public void Configure(EntityTypeBuilder<ActivityEvent> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.Timestamp })
            .HasDatabaseName("IX_ActivityEvents_UserId_Timestamp");
    }
}