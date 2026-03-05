using ECommercePlatform.Application.Mapping;
using ECommercePlatform.Application.Orders;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Shared; // Money, Currency
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentAssertions;
using GenericRepository;
using Mapster;
using MockQueryable;
using Moq;
using System.Reflection;

namespace ECommercePlatform.Application.Tests.Orders;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _orderRepoMock = new Mock<IRepository<Order>>();

        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        new MapsterConfig().Register(TypeAdapterConfig.GlobalSettings);

        _handler = new GetOrderByIdQueryHandler(_orderRepoMock.Object);
    }

    private void AddItemToOrder(Order order, OrderItem item)
    {
        var type = typeof(Order);
        var backingField = type.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<OrderItem>?)backingField?.GetValue(order);
        list?.Add(item);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_OrderNotFound()
    {
        _orderRepoMock.Setup(x => x.AsQueryable()).Returns(new List<Order>().BuildMock());

        var result = await _handler.Handle(new GetOrderByIdQuery("BOS-123"), CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Sipariş bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderDetail_With_FlattenedAddress_And_Items()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), new Address("İstanbul", "Kadıköy", "Moda", "34000", "Tam Adres"));

        var type = typeof(ECommercePlatform.Domain.Abstractions.Entity);
        type.GetProperty("Id")?.SetValue(order, Guid.NewGuid());

        var orderItem = new OrderItem(order.Id, Guid.NewGuid(), "Laptop", new Money(1000, Currency.TRY), 2);
        AddItemToOrder(order, orderItem);

        var list = new List<Order> { order };
        _orderRepoMock.Setup(x => x.AsQueryable()).Returns(list.BuildMock());

        var result = await _handler.Handle(new GetOrderByIdQuery(order.OrderNumber), CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Adres Mapster ile başarılı flatten edildi mi? (ShippingAddress.City -> ShippingCity)
        result.Data!.ShippingCity.Should().Be("İstanbul");

        // Total Amount ve Item'lar geldi mi? (1000 * 2 = 2000)
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().ProductName.Should().Be("Laptop");
        result.Data.TotalAmount.Should().Be(2000);
    }
}
