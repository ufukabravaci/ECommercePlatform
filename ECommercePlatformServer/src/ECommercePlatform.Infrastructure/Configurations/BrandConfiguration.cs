using ECommercePlatform.Domain.Brands;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LogoUrl).HasMaxLength(500).IsRequired(false);

        // Indexes
        builder.HasIndex(x => x.CompanyId);

        // Aynı şirket içinde aynı marka isminden 2 tane olmasın (Soft Delete hariç)
        builder.HasIndex(x => new { x.CompanyId, x.Name })
               .IsUnique()
               .HasFilter("[IsDeleted] = 0");
    }
}
