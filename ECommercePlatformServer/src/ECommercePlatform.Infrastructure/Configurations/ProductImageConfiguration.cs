using ECommercePlatform.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.IsMain)
               .HasDefaultValue(false) // Varsayılan değer
               .IsRequired();

        // ProductId zaten ProductConfiguration tarafında tanımlandı (HasForeignKey).
        // Ancak explicit (açık) olarak burada da belirtmek okunabilirliği artırır, zorunlu değildir.
        // builder.HasOne<Product>().WithMany(p => p.Images).HasForeignKey(x => x.ProductId);
    }
}