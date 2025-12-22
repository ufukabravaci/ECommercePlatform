using ECommercePlatform.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class CompanyUserConfiguration
    : IEntityTypeConfiguration<CompanyUser>
{
    public void Configure(EntityTypeBuilder<CompanyUser> builder)
    {
        builder.ToTable("CompanyUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.CompanyId).IsRequired();

        // --- İLİŞKİLER ---

        // 1. User İlişkisi (DÜZELTİLDİ)
        builder.HasOne(x => x.User)
               .WithMany(u => u.CompanyUsers) // User tarafındaki listeyi gösterdik
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // 2. Company İlişkisi
        // Eğer Company entity'sinde "CompanyUsers" listesi varsa:
        builder.HasOne(x => x.Company)
               .WithMany(c => c.CompanyUsers)
               .HasForeignKey(x => x.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);

        // Eğer Company entity'sinde liste YOKSA (Sadece CompanyUser -> Company olsun istiyorsan):
        // .WithMany() // Boş bırakılır.

        // Index
        builder.HasIndex(x => new { x.UserId, x.CompanyId }).IsUnique();
        builder.HasIndex(x => x.CompanyId); // Tenant sorguları için performans

        // --- ROLES MAPPING (DÜZELTİLDİ) ---
        // Backing Field "_roles" olsa da Property "Roles" üzerinden konfigüre ediyoruz.

        builder.Property(x => x.Roles)
       .HasColumnName("Roles")
       .UsePropertyAccessMode(PropertyAccessMode.Field)
       .HasConversion(
           v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
           v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
       )
       .HasColumnType("nvarchar(max)")
       .Metadata.SetValueComparer(new ValueComparer<IReadOnlyCollection<string>>(
           // 1. Eşitlik Kontrolü (Null-Safe hale getirildi)
           (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),

           // 2. Hash Code (Null ise 0 dön, değilse hesapla)
           c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),

           // 3. Snapshot (Clone)
           c => c == null ? new List<string>() : c.ToList()));
    }
}
