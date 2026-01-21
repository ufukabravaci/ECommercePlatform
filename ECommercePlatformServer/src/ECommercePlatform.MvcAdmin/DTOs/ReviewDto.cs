namespace ECommercePlatform.MvcAdmin.DTOs;

public sealed class ReviewDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = default!;
    public int Rating { get; set; }
    public string Comment { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public string? SellerReply { get; set; }
    public DateTimeOffset? SellerRepliedAt { get; set; }

    // Ek alanlar (Product Index için)
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public bool IsApproved { get; set; }
}