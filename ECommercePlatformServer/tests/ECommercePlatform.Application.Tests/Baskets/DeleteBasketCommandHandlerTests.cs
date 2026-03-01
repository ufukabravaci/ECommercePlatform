using ECommercePlatform.Application.Baskets;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Baskets;
using FluentAssertions;
using Moq;

namespace ECommercePlatform.Application.Tests.Baskets;

public class DeleteBasketCommandHandlerTests
{
    private readonly Mock<IBasketRepository> _basketRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly DeleteBasketCommandHandler _handler;

    public DeleteBasketCommandHandlerTests()
    {
        _basketRepositoryMock = new Mock<IBasketRepository>();
        _userContextMock = new Mock<IUserContext>();

        _handler = new DeleteBasketCommandHandler(
            _basketRepositoryMock.Object,
            _userContextMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldDeleteBasket_And_ReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        _basketRepositoryMock
            .Setup(x => x.DeleteBasketAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new DeleteBasketCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Sepet temizlendi.");

        _basketRepositoryMock.Verify(x => x.DeleteBasketAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
