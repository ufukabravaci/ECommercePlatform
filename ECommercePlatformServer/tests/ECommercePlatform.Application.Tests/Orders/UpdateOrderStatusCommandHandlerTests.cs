using ECommercePlatform.Application.Orders;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Orders;

public class UpdateOrderStatusCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateOrderStatusCommandHandler _handler;

    public UpdateOrderStatusCommandHandlerTests()
    {
        _orderRepoMock = new Mock<IRepository<Order>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new UpdateOrderStatusCommandHandler(_orderRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_OrderNotFound()
    {
        _orderRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Order)null!);

        var command = new UpdateOrderStatusCommand("ORD-123", OrderStatus.Shipped);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Sipariş bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateStatusAndSave_When_Valid()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), new Address("C", "D", "S", "Z", "F"));
        // Varsayılan Pending

        _orderRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(order);

        var command = new UpdateOrderStatusCommand(order.OrderNumber, OrderStatus.Processing);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Sipariş durumu güncellendi.");

        order.Status.Should().Be(OrderStatus.Processing);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
