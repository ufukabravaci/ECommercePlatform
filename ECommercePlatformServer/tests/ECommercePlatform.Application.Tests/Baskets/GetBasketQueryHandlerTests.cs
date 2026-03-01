using ECommercePlatform.Application.Baskets;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Baskets;
using FluentAssertions;
using Moq;

namespace ECommercePlatform.Application.Tests.Baskets;

public class GetBasketQueryHandlerTests
{
    private readonly Mock<IBasketRepository> _basketRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly GetBasketQueryHandler _handler;

    public GetBasketQueryHandlerTests()
    {
        _basketRepositoryMock = new Mock<IBasketRepository>();
        _userContextMock = new Mock<IUserContext>();

        _handler = new GetBasketQueryHandler(
            _basketRepositoryMock.Object,
            _userContextMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyBasket_When_BasketIsNull()
    {
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        // Repository null dönüyor (Cache'te yok)
        _basketRepositoryMock.Setup(x => x.GetBasketAsync(userId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync((CustomerBasket?)null);

        var query = new GetBasketQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CustomerId.Should().Be(userId);
        result.Data.Items.Should().BeEmpty(); // Boş sepet oluşmuş olmalı
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingBasket_When_BasketExists()
    {
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        var existingBasket = new CustomerBasket(userId)
        {
            Items = new List<BasketItem> { new BasketItem { ProductId = Guid.NewGuid(), Quantity = 1 } }
        };

        _basketRepositoryMock.Setup(x => x.GetBasketAsync(userId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(existingBasket);

        var query = new GetBasketQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CustomerId.Should().Be(userId);
        result.Data.Items.Should().HaveCount(1);
    }
}
