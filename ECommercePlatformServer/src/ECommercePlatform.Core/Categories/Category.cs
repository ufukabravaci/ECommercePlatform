using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Companies;
using System.Text.RegularExpressions;

namespace ECommercePlatform.Domain.Categories;

public sealed class Category : Entity, IMultiTenantEntity
{
    private Category() { }

    public Category(string name, Guid companyId) : this()
    {
        if (companyId == Guid.Empty) throw new ArgumentException("Kategori bir şirkete ait olmalıdır.");
        UpdateName(name);
        CompanyId = companyId;
    }

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!; // url-dostu-isim
    public Guid? ParentId { get; private set; }
    public Guid CompanyId { get; private set; }

    public Category? Parent { get; private set; }
    public Company Company { get; private set; } = default!;
    private readonly List<Category> _subCategories = new();
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    #region Behaviors/Methods
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Kategori adı boş olamaz.");

        Name = name.Trim();
        Slug = GenerateSlug(Name);
    }
    public void SetParent(Category parent)
    {
        if (parent is null)
            throw new ArgumentNullException(nameof(parent));

        if (parent.Id == Id)
            throw new ArgumentException("Bir kategori kendisinin üst kategorisi olamaz.");

        if (parent.CompanyId != CompanyId)
            throw new InvalidOperationException("Üst kategori farklı şirkete ait olamaz.");

        if (parent.IsDescendantOf(this))
            throw new InvalidOperationException("Döngüsel kategori ilişkisi oluşturulamaz.");

        Parent = parent;
        ParentId = parent.Id;
    }
    public void RemoveParent()
    {
        Parent = null;
        ParentId = null;
    }

    public void AddSubCategory(Category child)
    {
        if (child is null)
            throw new ArgumentNullException(nameof(child));

        if (child.CompanyId != CompanyId)
            throw new InvalidOperationException("Alt kategori farklı şirkete ait olamaz.");

        if (child.Id == Id)
            throw new InvalidOperationException("Kategori kendisine alt kategori olamaz.");

        if (IsDescendantOf(child))
            throw new InvalidOperationException("Döngüsel kategori ilişkisi oluşturulamaz.");

        child.SetParent(this);
        _subCategories.Add(child);
    }
    private bool IsDescendantOf(Category category)
    {   //A => B => C(this) => A eklenmemeli o yüzden c'nin parentlarına bakacağız.
        var current = Parent; // 1 => current = b // 2 => current = a
        while (current != null)
        {
            if (current.Id == category.Id)  // 1 => b.Id == a.Id // 2 => a.Id == a.Id true
                return true;

            current = current.Parent; // 1 =>current = a // 2 => current = null
        }
        return false;
    }
    private string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        text = text.ToLowerInvariant();

        // Türkçe Karakter Dönüşümü
        text = text.Replace('ı', 'i').Replace('ğ', 'g').Replace('ü', 'u')
                   .Replace('ş', 's').Replace('ö', 'o').Replace('ç', 'c');

        // Geçersiz karakterleri temizle
        text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
        // Birden fazla boşluğu teke indir
        text = Regex.Replace(text, @"\s+", " ").Trim();
        // Boşlukları tire yap
        text = text.Replace(' ', '-');

        return text;
    }
    #endregion
}
