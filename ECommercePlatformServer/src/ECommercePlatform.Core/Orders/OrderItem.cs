using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Shared;

namespace ECommercePlatform.Domain.Orders;

public sealed class OrderItem : Entity
{
    private OrderItem() { }

    public OrderItem(Guid orderId, Guid productId, string productName, Money price, int quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName; // Snapshot (Ürün adı değişse bile siparişte sabit kalır)
        Price = price;             // Snapshot (Fiyat değişse bile sabit kalır)
        Quantity = quantity;
    }

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public Money Price { get; private set; } = default!; // Value Object
    public int Quantity { get; private set; }
}