using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReUse.Domain.Entities;
using ReUse.Domain.Enums;

namespace ReUse.Infrastructure.Persistence.Configurations;

public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.Description)
            .IsRequired();

        builder.Property(p => p.ProductType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Condition)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(ProductStatus.Active);

        builder.Property(p => p.IsPremium)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.PremiumExpiresAt);

        builder.Property(p => p.ViewCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.RecentFavoriteCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.LocationCity)
            .HasMaxLength(100);

        builder.Property(p => p.LocationCountry)
            .HasMaxLength(100);

        builder.HasDiscriminator(p => p.ProductType)
            .HasValue<RegularProduct>(ProductType.Regular)
            .HasValue<WantedProduct>(ProductType.Wanted)
            .HasValue<SwapProduct>(ProductType.Swap);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasOne(p => p.Owner)
            .WithMany()
            .HasForeignKey(p => p.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.OwnerUserId);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Title);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.ProductType);

        builder.HasIndex(p => new { p.Status, p.RecentFavoriteCount })
            .HasDatabaseName("ix_products_status_favcount");

        builder.HasIndex(p => new { p.Status, p.CreatedAt })
            .HasDatabaseName("ix_products_status_created");

        builder.HasIndex(p => new { p.Status, p.LocationCountry, p.LocationCity })
            .HasDatabaseName("ix_products_status_location");

        builder.HasIndex(p => new { p.IsPremium, p.PremiumExpiresAt })
            .HasDatabaseName("ix_products_premium");
    }
}