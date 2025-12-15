using ECommercePlatform.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(150).IsRequired();
        // Kategori üzerinden ilişki kuramıyoruz çünkü Products koleksiyonu category tarafında tanımlı değil.

        builder.HasOne(c => c.Company)
               .WithMany() // Company içinde "Categories" listesi olmadığı için boş bırakıyoruz
               .HasForeignKey(c => c.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);

        // Index
        builder.HasIndex(c => c.CompanyId);

        // Slug (Url) benzersiz olmalı ama "Şirket Bazında" benzersiz olmalı
        // A şirketi "Elektronik" kategorisi açabilir, B şirketi de açabilir.
        // Bu yüzden Unique Index'i (Slug + CompanyId) üzerine kuruyoruz.
        builder.HasIndex(c => new { c.Slug, c.CompanyId }).IsUnique();
    }
}
