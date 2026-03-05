using ECommercePlatform.Application.Orders;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Orders;

public class AddTrackingNumberCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddTrackingNumberCommandHandler _handler;

    public AddTrackingNumberCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IRepository<Order>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new AddTrackingNumberCommandHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_OrderNotFound()
    {
        _orderRepositoryMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Order)null!);

        var command = new AddTrackingNumberCommand("ORD-123", "TRK-123");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Sipariş bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldAddTrackingNumber_When_Valid()
    {
        var fakeAddress = new Address("Şehir", "İlçe", "Sokak", "0000", "Tam");
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), fakeAddress);

        _orderRepositoryMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(order);

        var command = new AddTrackingNumberCommand(order.OrderNumber, "TRK-123");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Kargo takip numarası eklendi.");

        order.CargoTrackingNumber.Should().Be("TRK-123");
        order.Status.Should().Be(OrderStatus.Shipped);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
