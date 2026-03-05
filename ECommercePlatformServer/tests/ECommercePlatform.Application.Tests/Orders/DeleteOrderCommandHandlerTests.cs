using ECommercePlatform.Application.Orders;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Shared; // Money & Currency
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentAssertions;
using GenericRepository;
using MockQueryable;
using Moq;
using System.Reflection;

namespace ECommercePlatform.Application.Tests.Orders;

public class DeleteOrderCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock;
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteOrderCommandHandler _handler;

    public DeleteOrderCommandHandlerTests()
    {
        _orderRepoMock = new Mock<IRepository<Order>>();
        _productRepoMock = new Mock<IRepository<Product>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new DeleteOrderCommandHandler(
            _orderRepoMock.Object,
            _unitOfWorkMock.Object,
            _productRepoMock.Object
        );
    }

    private void SetEntityId(ECommercePlatform.Domain.Abstractions.Entity entity, Guid id)
    {
        var prop = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        prop?.SetValue(entity, id);
    }

    // Encapsulated olan _items listesine zorla eleman ekleyen yardımcı metot
    private void AddItemToOrder(Order order, OrderItem item)
    {
        var type = typeof(Order);
        var backingField = type.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<OrderItem>?)backingField?.GetValue(order);
        list?.Add(item);
    }

    [Fact]
    public async Task Handle_ShouldDeleteOrderAndRefundStock()
    {
        var companyId = Guid.NewGuid();
        var p1 = new Product("P1", "S1", "D1", new Money(100, Currency.TRY), 5, companyId, Guid.NewGuid(), Guid.NewGuid());
        SetEntityId(p1, Guid.NewGuid());

        var order = new Order(Guid.NewGuid(), companyId, new Address("C", "D", "S", "Z", "F"));
        SetEntityId(order, Guid.NewGuid());

        // Kullanıcı bu siparişte 3 adet P1 satın almıştı.
        var orderItem = new OrderItem(order.Id, p1.Id, p1.Name, p1.Price, 3);
        AddItemToOrder(order, orderItem);

        var orderList = new List<Order> { order };
        _orderRepoMock.Setup(x => x.AsQueryable()).Returns(orderList.BuildMock());

        var productList = new List<Product> { p1 };
        _productRepoMock.Setup(x => x.WhereWithTracking(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .Returns(productList.BuildMock());

        var command = new DeleteOrderCommand(order.OrderNumber);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Contain("iptal edildi");

        // Orijinal stok 5 idi. İptal edilen 3 ürün geri döndü. Stok 8 olmalı!
        p1.Stock.Should().Be(8);

        // Sipariş ve Kalemleri Silindi mi?
        order.IsDeleted.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);

        _productRepoMock.Verify(x => x.Update(p1), Times.Once);
        _orderRepoMock.Verify(x => x.Update(order), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
