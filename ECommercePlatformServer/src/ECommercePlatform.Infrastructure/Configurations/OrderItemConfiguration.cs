using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        // Tablo Adı
        builder.ToTable("OrderItems");

        // PK
        builder.Property(x => x.Id).ValueGeneratedNever();

        // --- Properties ---
        builder.Property(x => x.ProductName)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Quantity)
               .IsRequired();

        // --- Value Objects (Money) ---
        // Snapshot Fiyat. Ürün fiyatı değişse bile sipariş anındaki fiyat burada saklanır.
        builder.OwnsOne(x => x.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                        .HasColumnName("PriceAmount")
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();

            priceBuilder.Property(m => m.Currency)
                        .HasColumnName("PriceCurrency")
                        .HasMaxLength(5)
                        .IsRequired()
                        .HasConversion<string>();
        });

        // --- Relationships ---

        // 1. Order (Parent)
        // OrderConfiguration tarafında HasMany ile tanımlandı ama burada da belirtmek iyidir.
        builder.HasOne<Order>()
               .WithMany(o => o.Items)
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.Restrict);

        // 2. Product (Referans)
        // Ürün silinirse (Soft Delete) sipariş kalemi bozulmamalı. Restrict.
        // ProductId sadece referans tutar.
        builder.HasOne<Product>()
               .WithMany()
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        // --- Indexes ---
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.ProductId);
    }
}