using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReUse.Domain.Entities;

namespace ReUse.Infrastructure.Persistence.Configurations;

public class FavoriteEntityTypeConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("favorites");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
               .ValueGeneratedNever();

        builder.Property(f => f.UserId)
               .IsRequired();

        builder.Property(f => f.ProductId)
               .IsRequired();

        builder.HasIndex(f => new { f.UserId, f.ProductId })
               .IsUnique();

        builder.HasOne(f => f.User)
               .WithMany(u => u.Favorites)
               .HasForeignKey(f => f.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Product)
               .WithMany(p => p.Favorites)
               .HasForeignKey(f => f.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(f => f.CreatedAt)
               .IsRequired();
    }
}