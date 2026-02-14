namespace ECommercePlatform.Application.DTOs;

public sealed record ReviewDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    int Rating,
    string Comment,
    DateTimeOffset CreatedAt,
    string? SellerReply,
    DateTimeOffset? SellerRepliedAt
);
