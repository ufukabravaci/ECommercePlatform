using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Shared;
using ECommercePlatform.Domain.Users;
using ECommercePlatform.Domain.Users.ValueObjects;

namespace ECommercePlatform.Domain.Orders;

public sealed class Order : Entity, IMultiTenantEntity
{
    private Order()
    {
        _items = new List<OrderItem>();
    }

    public Order(Guid customerId, Guid companyId, Address shippingAddress) : this()
    {
        if (customerId == Guid.Empty) throw new ArgumentException("Müşteri bilgisi zorunludur.");
        if (companyId == Guid.Empty) throw new ArgumentException("Şirket bilgisi zorunludur.");

        CustomerId = customerId;
        CompanyId = companyId;
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));

        OrderNumber = GenerateOrderNumber();
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
    }

    public string OrderNumber { get; private set; } = default!;
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }

    // Multi-Tenant
    public Guid CompanyId { get; private set; }

    // Customer (User tablosu ile ilişkili)
    public Guid CustomerId { get; private set; }
    public User Customer { get; private set; } = default!;

    // Value Object
    public Address ShippingAddress { get; private set; } = default!;

    // Items
    private readonly List<OrderItem> _items;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }

    public void AddOrderItem(Guid productId, string productName, Money price, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Miktar en az 1 olmalıdır.");

        _items.Add(new OrderItem(Id, productId, productName, price, quantity));
    }

    public decimal CalculateTotalAmount()
    {
        if (!_items.Any()) return 0;
        return _items.Sum(x => x.Price.Amount * x.Quantity);
    }

    private string GenerateOrderNumber()
    {
        // Örn: ORD-20231025-X9Y2
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
    }

    // SOFT DELETE BEHAVIOR
    public override void Delete()
    {
        // 1. Kendini sil (Soft)
        // Entity sınıfındaki base Delete() metodunu çağırıyoruz
        base.Delete();

        // 2. Çocuklarını da sil (Soft)
        // Eğer bir sipariş mantıksal olarak silindiyse, kalemleri de silinmiş görünmelidir.
        foreach (var item in _items)
        {
            item.Delete(); // OrderItem da Entity'den türediği için bu metoda sahip.
        }

        // Ekstra: Status güncellemesi
        Status = OrderStatus.Cancelled;
    }
}