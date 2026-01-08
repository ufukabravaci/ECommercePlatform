using ECommercePlatform.Domain.Abstractions;
namespace ECommercePlatform.Domain.Brands;

public sealed class Brand : Entity, IMultiTenantEntity
{
    private Brand() { }

    public Brand(string name, string? logoUrl, Guid companyId)
    {
        SetName(name);
        LogoUrl = logoUrl; // Opsiyonel

        if (companyId == Guid.Empty) throw new ArgumentException("Şirket bilgisi zorunludur.");
        CompanyId = companyId;
    }

    public string Name { get; private set; } = default!;
    public string? LogoUrl { get; private set; }

    // Multi-Tenant
    public Guid CompanyId { get; private set; }

    // Relations
    // Bir markanın birden fazla ürünü olabilir.
    // Ancak Product tarafında BrandId zorunlu olacaksa, silme işlemi Restrict olmalı.
    // Collection'ı burada tanımlamak zorunda değiliz (Perf için), ama Navigation olarak koyalım.
    // private readonly List<Product> _products = new();
    // public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    // --- BEHAVIORS ---
    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            throw new ArgumentException("Marka adı en az 2 karakter olmalıdır.");
        Name = name.Trim();
    }

    public void Update(string name, string? logoUrl)
    {
        SetName(name);
        LogoUrl = logoUrl;
    }
}
