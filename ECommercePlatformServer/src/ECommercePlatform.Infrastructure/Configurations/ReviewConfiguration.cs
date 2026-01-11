using ECommercePlatform.Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Comment).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.SellerReply).HasMaxLength(1000).IsRequired(false);
        builder.Property(x => x.Rating).IsRequired();

        // Relations
        builder.HasOne(x => x.Product)
               .WithMany() // Product içinde reviews listesi tutmuyoruz (Lazy loading riski)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Cascade); // Ürün silinirse yorumlar da gitsin

        builder.HasOne(x => x.Customer)
               .WithMany()
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict); // Müşteri silinse de yorum kalsın

        // Indexes
        builder.HasIndex(x => x.ProductId); // Ürün detay sayfasında yorumları çekmek için
        builder.HasIndex(x => x.CompanyId); // Admin panelde tüm yorumları görmek için
    }
}
