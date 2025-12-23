using ECommercePlatform.Domain.Abstractions;

namespace ECommercePlatform.Domain.Products;

public sealed class ProductImage : Entity
{
    private ProductImage() { } // EF Core için

    internal ProductImage(string imageUrl, bool isMain)
    {
        ImageUrl = imageUrl;
        IsMain = isMain;
    }

    public Guid ProductId { get; private set; }
    public string ImageUrl { get; private set; } = default!;
    public bool IsMain { get; private set; }

    internal void SetMain(bool isMain) => IsMain = isMain;
}