using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Companies;

namespace ECommercePlatform.Domain.Categories;

public sealed class Category : Entity, IMultiTenantEntity
{
    private Category() { }

    public Category(string name, Guid companyId) : this()
    {
        SetName(name);
        if (companyId == Guid.Empty) throw new ArgumentException("Kategori bir şirkete ait olmalıdır.");
    }

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!; // url-dostu-isim

    public Guid CompanyId { get; private set; }
    public Company Company { get; set; } = default!;

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Kategori adı boş olamaz.");
        Name = name;
        // Daha detaylı kurallar eklenebilir
        Slug = name.ToLowerInvariant().Replace(" ", "-")
                   .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u")
                   .Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
    }
}
