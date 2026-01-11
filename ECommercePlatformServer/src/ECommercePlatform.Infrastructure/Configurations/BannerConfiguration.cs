using ECommercePlatform.Domain.Banners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.ToTable("Banners");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.ImageUrl).IsRequired();
        builder.Property(x => x.TargetUrl).HasMaxLength(500).IsRequired(false);

        // Tenant ve Sıralama performansı için index
        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => new { x.CompanyId, x.Order }); // Sıralı çekim için
    }
}
