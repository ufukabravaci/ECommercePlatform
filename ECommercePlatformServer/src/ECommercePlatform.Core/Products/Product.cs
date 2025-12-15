using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Shared;

namespace ECommercePlatform.Domain.Products;

public sealed class Product : Entity, IMultiTenantEntity
{
    private Product() { }

    public Product(
        string name,
        Money price,
        int stock,
        Guid companyId,
        Guid categoryId)
    {
        SetName(name);
        SetPrice(price);
        UpdateStock(stock);

        if (companyId == Guid.Empty) throw new ArgumentException("Ürün bir şirkete ait olmalıdır.");
        if (categoryId == Guid.Empty) throw new ArgumentException("Ürün bir kategoriye ait olmalıdır.");

        CompanyId = companyId;
        CategoryId = categoryId;
    }

    public string Name { get; private set; } = default!;
    public Money Price { get; private set; } = default!;
    public int Stock { get; private set; }

    // Foreign Keys
    public Guid CompanyId { get; private set; }
    public Company Company { get; set; } = default!;

    public Guid CategoryId { get; private set; }
    public Category Category { get; set; } = default!;

    #region Methods
    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            throw new ArgumentException("Ürün adı geçersiz.");
        Name = name;
    }

    public void SetPrice(Money price)
    {
        Price = price ?? throw new ArgumentNullException(nameof(price));
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Stok adedi negatif olamaz.");
        Stock = quantity;
    }
    #endregion
}
