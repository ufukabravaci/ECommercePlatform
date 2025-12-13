using ECommercePlatform.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Stock).IsRequired();

        // MONEY Value Object Mapping
        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                        .HasColumnName("PriceAmount")
                        .HasColumnType("decimal(18,2)") // Para için hassasiyet önemli
                        .IsRequired();

            priceBuilder.Property(m => m.Currency)
                        .HasColumnName("PriceCurrency")
                        .HasMaxLength(5)
                        .IsRequired()
                        .HasConversion<string>();
        });

        // İlişkiler
        // Company (Tenant) İlişkisi
        builder.HasOne(p => p.Company)
               .WithMany()
               .HasForeignKey(p => p.CompanyId)
               .OnDelete(DeleteBehavior.Cascade); // Şirket silinirse ürünleri de silinsin.

        // Category İlişkisi
        builder.HasOne(p => p.Category)
               .WithMany()
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict); // Kategori silinemez, ürünler etkilenmez.

        // Indexler (Performans için)
        builder.HasIndex(p => p.CompanyId);
        builder.HasIndex(p => p.CategoryId);
    }
}
