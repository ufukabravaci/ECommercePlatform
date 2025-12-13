using ECommercePlatform.Domain.Abstractions;

namespace ECommercePlatform.Domain.Categories;

public sealed class Category : Entity
{
    private Category() { }

    public Category(string name) : this()
    {
        SetName(name);
    }

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!; // url-dostu-isim

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
