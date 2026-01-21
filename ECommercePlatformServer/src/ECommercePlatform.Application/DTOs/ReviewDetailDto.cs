namespace ECommercePlatform.Application.DTOs;

public sealed record ReviewDetailDto(
    Guid Id,
    string CustomerName,
    int Rating,
    string Comment,
    DateTimeOffset CreatedAt,
    string? SellerReply,
    DateTimeOffset? SellerRepliedAt,
    Guid ProductId,
    string ProductName,
    bool IsApproved
);
