using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Shared;

namespace ECommercePlatform.Domain.Products;

public sealed class Product : Entity, IMultiTenantEntity
{
    private Product()
    {
        _images = new List<ProductImage>();
    }

    public Product(
        string name,
        string sku,
        string description,
        Money price,
        int stock,
        Guid companyId,
        Guid categoryId) : this()
    {
        SetName(name);
        SetSku(sku);
        SetDescription(description);
        SetPrice(price);
        UpdateStock(stock);
        SetCategory(categoryId);

        if (companyId == Guid.Empty) throw new ArgumentException("Şirket bilgisi zorunludur.");

        CompanyId = companyId;
    }

    // Properties
    public string Name { get; private set; } = default!;
    public string Sku { get; private set; } = default!; // Stok Kodu
    public string Description { get; private set; } = default!;
    public Money Price { get; private set; } = default!;
    public int Stock { get; private set; }

    // Relations
    public Guid CompanyId { get; private set; }
    public Company Company { get; set; } = default!;
    public Guid CategoryId { get; private set; }
    public Category Category { get; set; } = default!;

    // Encapsulated Collection
    private readonly List<ProductImage> _images;
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    // --- BEHAVIORS ---

    public void AddImage(string imageUrl, bool isMain = false)
    {
        if (_images.Count >= 20)
            throw new InvalidOperationException("Bir ürüne en fazla 20 resim eklenebilir.");

        // Eğer hiç resim yoksa, eklenen ilk resim otomatik Main olur.
        if (_images.Count == 0) isMain = true;

        if (isMain)
        {
            // Diğerlerinin main özelliğini kaldır
            foreach (var img in _images) img.SetMain(false);
        }

        _images.Add(new ProductImage(imageUrl, isMain));
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            throw new ArgumentException("Ürün adı en az 2 karakter olmalıdır.");
        Name = name.Trim();
    }

    public void SetSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU boş olamaz.");
        Sku = sku.Trim().ToUpperInvariant();
    }

    public void SetPrice(Money price) => Price = price ?? throw new ArgumentNullException(nameof(price));

    public void UpdateStock(int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Stok negatif olamaz.");
        Stock = quantity;
    }
    public void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Ürün açıklaması boş olamaz.");

        if (description.Length > 2000)
            throw new ArgumentException("Ürün açıklaması 2000 karakteri geçemez.");

        Description = description.Trim();
    }
    public void RemoveImage(Guid imageId)
    {
        // 1. Resmi bul (Sadece silinmemişler arasında ara)
        var image = _images.FirstOrDefault(x => x.Id == imageId && !x.IsDeleted);

        if (image is null)
            throw new InvalidOperationException("Silinmek istenen resim bulunamadı.");

        // 2. Eğer silinen resim "Main" (Ana Resim) ise yeni bir ana resim seçmemiz lazım
        bool wasMain = image.IsMain;

        // 3. HARD DELETE YERİNE SOFT DELETE
        image.Delete();         // <-- YENİ KOD (IsDeleted = true yapar)
        image.SetMain(false);   // Silinen resim artık Main olamaz

        // 4. Yeni Ana Resim Seçimi
        if (wasMain)
        {
            // Silinmemiş (IsDeleted == false) ilk resmi bul
            var nextMainImage = _images.FirstOrDefault(x => !x.IsDeleted && x.Id != imageId);

            if (nextMainImage != null)
            {
                nextMainImage.SetMain(true);
            }
        }
    }
    public void RemoveImageByUrl(string imageUrl)
    {
        var image = _images.FirstOrDefault(x => x.ImageUrl == imageUrl);
        if (image is null)
            throw new InvalidOperationException("Resim bulunamadı.");

        RemoveImage(image.Id);
    }

    public void SetMainImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(x => x.Id == imageId);
        if (image is null)
            throw new InvalidOperationException("Resim bulunamadı.");

        foreach (var img in _images)
            img.SetMain(false);

        image.SetMain(true);
    }

    public void SetCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Kategori bilgisi zorunludur.");

        CategoryId = categoryId;
    }
}