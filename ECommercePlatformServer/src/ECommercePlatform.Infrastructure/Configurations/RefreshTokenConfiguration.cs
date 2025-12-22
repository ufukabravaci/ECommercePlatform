using ECommercePlatform.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("UserRefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Expiration).IsRequired();
        builder.Property(x => x.ReplacedByToken).HasMaxLength(500);

        // 1. User İlişkisi
        builder.HasOne(x => x.User)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // 2. CompanyUser İlişkisi (BURAYI DEĞİŞTİRİYORUZ)
        // Döngüyü kırmak için burayı Restrict yapıyoruz.
        builder.HasOne(x => x.CompanyUser)
               .WithMany()
               .HasForeignKey(x => x.CompanyUserId)
               .OnDelete(DeleteBehavior.Restrict);
        // DİKKAT: CompanyUser silinirse token da silinmeli mi? 
        // Cascade yaparsak User silindiğinde çift cascade path hatası alabiliriz (SQL Server limitation).
        // Bu yüzden Restrict veya NoAction genelde daha güvenlidir, mantıksal silme (Soft Delete) kullanıyorsunuz zaten.

        // Indexler
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.UserId); // User Id ile hızlı arama
        builder.HasIndex(x => x.CompanyUserId); // Şirket bazlı sorgular için
    }
}