using ECommercePlatform.Application.Baskets;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Baskets;
using FluentAssertions;
using Moq;

namespace ECommercePlatform.Application.Tests.Baskets;

public class UpdateBasketCommandHandlerTests
{
    private readonly Mock<IBasketRepository> _basketRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly UpdateBasketCommandHandler _handler;

    public UpdateBasketCommandHandlerTests()
    {
        _basketRepositoryMock = new Mock<IBasketRepository>();
        _userContextMock = new Mock<IUserContext>();

        _handler = new UpdateBasketCommandHandler(
            _basketRepositoryMock.Object,
            _userContextMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateBasket_And_ReturnIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        var command = new UpdateBasketCommand(new List<BasketItem>
        {
            new BasketItem { ProductId = Guid.NewGuid(), Quantity = 2, PriceAmount = 50 }
        });

        // Repository'den dönen nesneyi simüle ediyoruz
        _basketRepositoryMock
            .Setup(x => x.UpdateBasketAsync(It.IsAny<CustomerBasket>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerBasket b, CancellationToken ct) => b); // Giren objeyi aynen dön

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CustomerId.Should().Be(userId);
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items[0].Quantity.Should().Be(2);

        // Repository update çağrıldı mı?
        _basketRepositoryMock.Verify(x => x.UpdateBasketAsync(
            It.Is<CustomerBasket>(b => b.CustomerId == userId && b.Items.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
