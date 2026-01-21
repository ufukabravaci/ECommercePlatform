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

        builder.Property(x => x.Id)
               .ValueGeneratedNever(); ;

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.CompanyId).IsRequired();

        // --- İLİŞKİLER ---

        // 1. User İlişkisi (DÜZELTİLDİ)
        builder.HasOne(x => x.User)
               .WithMany(u => u.CompanyUsers) // User tarafındaki listeyi gösterdik
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // 2. Company İlişkisi
        builder.HasOne(x => x.Company)
               .WithMany(c => c.CompanyUsers)
               .HasForeignKey(x => x.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);

        // Index
        builder.HasIndex(x => new { x.UserId, x.CompanyId }).IsUnique();
        builder.HasIndex(x => x.CompanyId); // Tenant sorguları için performans

        // --- ROLES MAPPING ---
        // Backing Field "_roles" olsa da Property "Roles" üzerinden konfigüre ediyoruz.
        // listeyi public yapmadan EF Core'un erişip değişiklik yapabilmesi için.
        builder.Property(x => x.Roles)
            .HasColumnName("Roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            )
            .HasColumnType("nvarchar(max)")
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyCollection<string>>(
                (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? new List<string>() : c.ToList()));

        // --- PERMISSIONS MAPPING ---
        builder.Property(x => x.Permissions)
            .HasColumnName("Permissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            )
            .HasColumnType("nvarchar(max)")
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyCollection<string>>(
                (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? new List<string>() : c.ToList()));
    }
}
