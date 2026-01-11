using ECommercePlatform.Domain.Abstractions;

namespace ECommercePlatform.Domain.Banners;

public sealed class Banner : Entity, IMultiTenantEntity
{
    private Banner() { }

    public Banner(
        string title,
        string description,
        string imageUrl,
        string targetUrl,
        int order,
        Guid companyId)
    {
        Title = title;
        Description = description;
        ImageUrl = imageUrl;
        TargetUrl = targetUrl; // Tıklanınca gideceği sayfa (örn: /products/123)
        Order = order;

        if (companyId == Guid.Empty) throw new ArgumentException("Şirket bilgisi zorunludur.");
        CompanyId = companyId;
    }

    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string ImageUrl { get; private set; } = default!;
    public string TargetUrl { get; private set; } = default!;
    public int Order { get; private set; }

    public Guid CompanyId { get; private set; }

    // Behaviors
    public void Update(string title, string description, string? imageUrl, string targetUrl, int order)
    {
        Title = title;
        Description = description;
        TargetUrl = targetUrl;
        Order = order;

        // Eğer yeni resim geldiyse güncelle, yoksa eskisi kalsın.
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            ImageUrl = imageUrl;
        }
    }
}