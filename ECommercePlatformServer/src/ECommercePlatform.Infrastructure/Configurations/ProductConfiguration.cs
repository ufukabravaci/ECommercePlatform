using ECommercePlatform.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Tablo Adı
        builder.ToTable("Products");

        // Primary Key
        builder.HasKey(p => p.Id);

        // --- Properties ---
        builder.Property(p => p.Name)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(p => p.Sku)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(p => p.Description)
               .HasMaxLength(2000)
               .IsRequired(false); // Nullable olabilir

        builder.Property(p => p.Stock)
               .IsRequired();

        // --- Value Objects (Money) ---
        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                        .HasColumnName("PriceAmount")
                        .HasColumnType("decimal(18,2)") // Para hassasiyeti
                        .IsRequired();

            priceBuilder.Property(m => m.Currency)
                        .HasColumnName("PriceCurrency")
                        .HasMaxLength(5)
                        .IsRequired()
                        .HasConversion<string>(); // Enum'ı string olarak sakla (TRY, USD)
        });

        // --- Relationships ---

        // 1. Company (Tenant) - Zorunlu ve Cascade
        builder.HasOne(p => p.Company)
               .WithMany() // Company tarafında Products listesi tutmuyoruz (Performance)
               .HasForeignKey(p => p.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);

        // 2. Category - Zorunlu ve Restrict
        // Kategori silinirse ürünler silinmemeli, hata vermeli.
        builder.HasOne(p => p.Category)
               .WithMany() // Category tarafında Products listesi yoksa
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        // 3. ProductImages (Child Collection)
        builder.HasMany(p => p.Images)
               .WithOne()
               .HasForeignKey(img => img.ProductId)
               .OnDelete(DeleteBehavior.Cascade); // Ürün silinirse resimleri çöp olur, silinsin.

        // --- ENCAPSULATION AYARI (Kritik) ---
        // Domain'de "private readonly List<ProductImage> _images" kullandığımız için
        // EF Core'a "Properties üzerinden değil, Field üzerinden git" diyoruz.
        builder.Metadata.FindNavigation(nameof(Product.Images))!
               .SetPropertyAccessMode(PropertyAccessMode.Field);

        // --- Indexes ---

        // Tenant filtreleri için
        builder.HasIndex(p => p.CompanyId);

        // Kategori filtreleri için
        builder.HasIndex(p => p.CategoryId);

        // SKU Benzersizliği (Tenant Bazlı)
        // A firmasının "KLM-01" ürünü olabilir, B firmasının da olabilir.
        // Ama A firması iki tane "KLM-01" oluşturamaz.
        builder.HasIndex(p => new { p.Sku, p.CompanyId })
               .IsUnique()
               .HasFilter("[IsDeleted] = 0"); // Sadece silinmemişlerde unique olsun (Soft Delete)

        builder.HasIndex(p => new { p.CompanyId, p.Name });
    }
}