using ECommercePlatform.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class CompanyInvitationConfiguration : IEntityTypeConfiguration<CompanyInvitation>
{
    public void Configure(EntityTypeBuilder<CompanyInvitation> builder)
    {
        builder.ToTable("CompanyInvitations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Email)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(x => x.Token)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Role)
          .IsRequired()
          .HasMaxLength(50);

        // Token ile sorgulama yapacağımız için Index atıyoruz (Performans için kritik)
        builder.HasIndex(x => x.Token).IsUnique();

        // Tenant performansı için
        builder.HasIndex(x => x.CompanyId);

        // Şirket İlişkisi
        builder.HasOne<Company>()
               .WithMany()
               .HasForeignKey(x => x.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}