using ECommercePlatform.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.Property(x => x.Id)
               .ValueGeneratedNever();

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

        builder.HasMany(u => u.RefreshTokens)
               .WithOne(rt => rt.User)
               .HasForeignKey(rt => rt.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // 2. CompanyUsers (1-N)
        builder.HasMany(u => u.CompanyUsers)
               .WithOne(cu => cu.User)
               .HasForeignKey(cu => cu.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // --- BACKING FIELD AYARLARI ---

        // RefreshTokens için field kullanımı
        builder.Metadata.FindNavigation(nameof(User.RefreshTokens))!
               .SetPropertyAccessMode(PropertyAccessMode.Field); // _refreshTokens fieldını otomatik bulur

        // CompanyUsers için field kullanımı
        builder.Metadata.FindNavigation(nameof(User.CompanyUsers))!
               .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
