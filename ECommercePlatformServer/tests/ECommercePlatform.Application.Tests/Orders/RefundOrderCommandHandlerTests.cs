using ECommercePlatform.Application.Orders;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Shared;
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentAssertions;
using GenericRepository;
using MockQueryable;
using Moq;
using System.Reflection;

namespace ECommercePlatform.Application.Tests.Orders;

public class RefundOrderCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock;
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RefundOrderCommandHandler _handler;

    public RefundOrderCommandHandlerTests()
    {
        _orderRepoMock = new Mock<IRepository<Order>>();
        _productRepoMock = new Mock<IRepository<Product>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RefundOrderCommandHandler(
            _orderRepoMock.Object,
            _productRepoMock.Object,
            _unitOfWorkMock.Object
        );
    }

    private void AddItemToOrder(Order order, OrderItem item)
    {
        var type = typeof(Order);
        var backingField = type.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<OrderItem>?)backingField?.GetValue(order);
        list?.Add(item);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_OrderAlreadyRefunded()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), new Address("C", "D", "S", "Z", "F"));
        order.UpdateStatus(OrderStatus.Refunded); // Zaten iade!

        var list = new List<Order> { order };
        _orderRepoMock.Setup(x => x.AsQueryable()).Returns(list.BuildMock());

        var result = await _handler.Handle(new RefundOrderCommand(order.OrderNumber), CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Sipariş zaten iade edilmiş.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateStatusAndRefundStock()
    {
        var productId = Guid.NewGuid();
        // Ürünün mevcut stoğu 10
        var product = new Product("P1", "S1", "D1", new Money(100, Currency.TRY), 10, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id")?.SetValue(product, productId);

        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), new Address("C", "D", "S", "Z", "F"));
        typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id")?.SetValue(order, Guid.NewGuid());

        // Kullanıcı 5 adet almıştı
        var orderItem = new OrderItem(order.Id, productId, "P1", new Money(100, Currency.TRY), 5);
        AddItemToOrder(order, orderItem);

        _orderRepoMock.Setup(x => x.AsQueryable()).Returns(new List<Order> { order }.BuildMock());
        _productRepoMock.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .Returns(new List<Product> { product }.BuildMock());

        var result = await _handler.Handle(new RefundOrderCommand(order.OrderNumber), CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Contain("iade alındı");

        // 10 Stok + İade Gelen 5 Adet = 15 Stok Olmalı
        product.Stock.Should().Be(15);
        order.Status.Should().Be(OrderStatus.Refunded);

        _orderRepoMock.Verify(x => x.Update(order), Times.Once);
        _productRepoMock.Verify(x => x.Update(product), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
