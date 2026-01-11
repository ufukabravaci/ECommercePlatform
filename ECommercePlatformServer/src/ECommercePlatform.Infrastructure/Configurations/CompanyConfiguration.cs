using ECommercePlatform.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(c => c.Id);
        builder.Property(x => x.Id)
               .ValueGeneratedNever();
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.TaxNumber)
            .IsRequired()
            .HasMaxLength(11);

        // Vergi numarası unique olmalı
        builder.HasIndex(c => c.TaxNumber).IsUnique();

        builder.OwnsOne(c => c.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.City).HasColumnName("City").HasMaxLength(100);
            addressBuilder.Property(a => a.District).HasColumnName("District").HasMaxLength(100);
            addressBuilder.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200);
            addressBuilder.Property(a => a.ZipCode).HasColumnName("ZipCode").HasMaxLength(20);
            addressBuilder.Property(a => a.FullAddress).HasColumnName("FullAddress").HasMaxLength(500);
        });
        builder.Navigation(u => u.Address).IsRequired(false);

        // 2. Shipping Settings Mapping
        builder.OwnsOne(c => c.ShippingSettings, settings =>
        {
            settings.Property(s => s.FreeShippingThreshold)
                .HasColumnName("FreeShippingThreshold")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            settings.Property(s => s.FlatRate)
                .HasColumnName("ShippingFlatRate")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);
        });
    }
}
