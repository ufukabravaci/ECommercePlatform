using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Users;

namespace ECommercePlatform.Domain.Reviews;

public sealed class Review : Entity, IMultiTenantEntity
{
    private Review() { }

    public Review(Guid productId, Guid customerId, Guid companyId, int rating, string comment)
    {
        if (rating < 1 || rating > 5) throw new ArgumentException("Puan 1 ile 5 arasında olmalıdır.");
        if (string.IsNullOrWhiteSpace(comment)) throw new ArgumentException("Yorum metni boş olamaz.");

        ProductId = productId;
        CustomerId = customerId;
        CompanyId = companyId;
        Rating = rating;
        Comment = comment;

        IsApproved = false; // Varsayılan onay bekler (Admin panelinden onaylanacak)
    }

    // Properties
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    public Guid CustomerId { get; private set; }
    public User Customer { get; private set; } = default!;

    public Guid CompanyId { get; private set; }

    public int Rating { get; private set; }
    public string Comment { get; private set; } = default!;

    public bool IsApproved { get; private set; }

    // Satıcı Cevabı
    public string? SellerReply { get; private set; }
    public DateTimeOffset? SellerRepliedAt { get; private set; }

    // Behaviors
    public void Approve()
    {
        IsApproved = true;
    }

    public void Reject()
    {
        base.Delete();
    }

    public void Reply(string reply)
    {
        if (string.IsNullOrWhiteSpace(reply)) throw new ArgumentException("Cevap metni boş olamaz.");

        SellerReply = reply;
        SellerRepliedAt = DateTimeOffset.Now;
        IsApproved = true; // Satıcı cevap verdiyse otomatik onaylanmış sayalım (Opsiyonel kural)
    }
}