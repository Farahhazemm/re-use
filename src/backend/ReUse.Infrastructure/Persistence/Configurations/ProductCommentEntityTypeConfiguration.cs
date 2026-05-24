using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReUse.Domain.Entities;

namespace ReUse.Infrastructure.Persistence.Configurations;

public class ProductCommentEntityTypeConfiguration : IEntityTypeConfiguration<ProductComment>
{
    public void Configure(EntityTypeBuilder<ProductComment> builder)
    {
        builder.ToTable("product_comments");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
               .ValueGeneratedNever();

        builder.Property(c => c.Body)
               .HasMaxLength(1000)
               .IsRequired();

        builder.Property(c => c.IsDeleted)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(c => c.DeletedAt);

        builder.Property(c => c.ParentCommentId);

        builder.Property(c => c.CreatedAt)
               .IsRequired();

        // Relationships
        builder.HasOne(c => c.Product)
               .WithMany(p => p.Comments)
               .HasForeignKey(c => c.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
               .WithMany()
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ParentComment)
               .WithMany(c => c.Replies)
               .HasForeignKey(c => c.ParentCommentId)
               .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        builder.HasIndex(c => new { c.ProductId, c.CreatedAt })
               .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(c => c.UserId);

        builder.HasIndex(c => c.ParentCommentId)
               .HasFilter("\"ParentCommentId\" IS NOT NULL");
    }
}