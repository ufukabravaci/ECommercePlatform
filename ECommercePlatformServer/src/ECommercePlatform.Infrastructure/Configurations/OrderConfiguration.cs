using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Tablo Adı
        builder.ToTable("Orders");

        // Primary Key
        builder.Property(x => x.Id)
               .ValueGeneratedNever();

        // --- Properties ---
        builder.Property(p => p.OrderNumber)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(p => p.OrderDate)
               .IsRequired();

        builder.Property(p => p.Status)
               .IsRequired();

        // --- Value Objects (Address) ---
        // ShippingAddress bir Value Object'tir. Tabloda kolon olarak (ShippingCity, ShippingStreet vb.) duracak.
        builder.OwnsOne(o => o.ShippingAddress, addressBuilder =>
        {
            addressBuilder.Property(a => a.City)
                .HasColumnName("ShippingCity")
                .HasMaxLength(100)
                .IsRequired();

            addressBuilder.Property(a => a.District)
                .HasColumnName("ShippingDistrict")
                .HasMaxLength(100)
                .IsRequired();

            addressBuilder.Property(a => a.Street)
                .HasColumnName("ShippingStreet")
                .HasMaxLength(200)
                .IsRequired();

            addressBuilder.Property(a => a.ZipCode)
                .HasColumnName("ShippingZipCode")
                .HasMaxLength(20)
                .IsRequired(false);

            addressBuilder.Property(a => a.FullAddress)
                .HasColumnName("ShippingFullAddress")
                .HasMaxLength(500)
                .IsRequired();
        });

        // --- Relationships ---

        // 1. Company (Tenant)
        builder.HasOne<Company>()
               .WithMany()
               .HasForeignKey(o => o.CompanyId)
               .OnDelete(DeleteBehavior.Restrict);

        // 2. Customer (User)
        // Müşteri silinse bile sipariş verisi kalmalı (Mali kayıt). O yüzden Restrict.
        builder.HasOne(o => o.Customer)
               .WithMany() // User tarafında Orders listesi tutmuyoruz.
               .HasForeignKey(o => o.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        // 3. OrderItems (Child Collection)
        builder.HasMany(o => o.Items)
               .WithOne()
               .HasForeignKey(i => i.OrderId)
               .OnDelete(DeleteBehavior.Restrict);

        // --- ENCAPSULATION AYARI ---
        // Domain'de "private readonly List<OrderItem> _items" var.
        builder.Metadata.FindNavigation(nameof(Order.Items))!
               .SetPropertyAccessMode(PropertyAccessMode.Field);

        // --- Indexes ---

        // Tenant performansı için
        builder.HasIndex(o => o.CompanyId);

        // "Siparişlerim" sayfası performansı için
        builder.HasIndex(o => o.CustomerId);

        // Sipariş Numarası Unique olmalı (Tenant bazında)
        builder.HasIndex(o => new { o.OrderNumber, o.CompanyId })
               .IsUnique()
               .HasFilter("[IsDeleted] = 0");
    }
}