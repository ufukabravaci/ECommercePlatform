namespace ECommercePlatform.Application.DTOs;

public sealed record ReviewDto(
    Guid Id,
    string CustomerName,
    int Rating,
    string Comment,
    DateTimeOffset CreatedAt,
    string? SellerReply,
    DateTimeOffset? SellerRepliedAt
);
