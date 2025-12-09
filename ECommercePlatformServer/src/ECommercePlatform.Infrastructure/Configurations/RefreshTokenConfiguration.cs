using ECommercePlatform.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("UserRefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
               .HasMaxLength(200) // Token uzunluğu genelde sabittir ama 200 güvenlidir
               .IsRequired();

        builder.Property(x => x.RevokedByIp)
               .HasMaxLength(50);

        builder.Property(x => x.ReplacedByToken)
               .HasMaxLength(200);

        // İlişkiler
        builder.HasOne(rt => rt.User)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(rt => rt.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Index (Token ile arama yapacağız)
        builder.HasIndex(x => x.Code);
    }
}
