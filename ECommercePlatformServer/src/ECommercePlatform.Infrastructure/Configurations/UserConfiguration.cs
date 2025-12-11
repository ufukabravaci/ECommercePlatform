using ECommercePlatform.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(p => p.FirstName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(p => p.LastName)
               .HasMaxLength(100)
               .IsRequired();

        builder.OwnsOne(u => u.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.City).HasColumnName("City").HasMaxLength(100);
            addressBuilder.Property(a => a.District).HasColumnName("District").HasMaxLength(100);
            addressBuilder.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200);
            addressBuilder.Property(a => a.ZipCode).HasColumnName("ZipCode").HasMaxLength(20);
            addressBuilder.Property(a => a.FullAddress).HasColumnName("FullAddress").HasMaxLength(500);
        });
        builder.Navigation(u => u.Address).IsRequired(false);

        // İlişkiler

        // RefreshToken İlişkisi (1-N)
        builder.HasMany(u => u.RefreshTokens)
               .WithOne(rt => rt.User)
               .HasForeignKey(rt => rt.UserId)
               .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silinirse tokenlar da silinsin.

        builder.HasOne(u => u.Company)          // User'ın bir Company'si var
               .WithMany(c => c.Users)          // Company'nin çok User'ı var
               .HasForeignKey(u => u.CompanyId) // Bağlantı anahtarı bu
               .OnDelete(DeleteBehavior.SetNull); // Şirket silinirse User boşa düşsün (veya Restrict diyerek engellenebilir)

        // Indexler
        // Email ve UserName zaten Identity tarafından indexli
        builder.HasIndex(x => x.CompanyId);
    }
}
