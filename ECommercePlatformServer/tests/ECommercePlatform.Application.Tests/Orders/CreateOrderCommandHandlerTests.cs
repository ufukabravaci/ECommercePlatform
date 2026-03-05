using ECommercePlatform.Application.Orders;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Baskets;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Shared; // Money & Currency
using FluentAssertions;
using GenericRepository;
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock;
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IBasketRepository> _basketRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepoMock = new Mock<IRepository<Order>>();
        _productRepoMock = new Mock<IRepository<Product>>();
        _basketRepoMock = new Mock<IBasketRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userContextMock = new Mock<IUserContext>();
        _tenantContextMock = new Mock<ITenantContext>();

        _handler = new CreateOrderCommandHandler(
            _orderRepoMock.Object,
            _productRepoMock.Object,
            _basketRepoMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object,
            _tenantContextMock.Object
        );
    }

    private void SetEntityId(ECommercePlatform.Domain.Abstractions.Entity entity, Guid id)
    {
        var prop = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        prop?.SetValue(entity, id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CurrenciesAreDifferent()
    {
        _userContextMock.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        _tenantContextMock.Setup(x => x.CompanyId).Returns(Guid.NewGuid());

        var p1 = new Product("P1", "S1", "D1", new Money(100, Currency.TRY), 10, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        SetEntityId(p1, Guid.NewGuid());

        var p2 = new Product("P2", "S2", "D2", new Money(100, Currency.USD), 10, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        SetEntityId(p2, Guid.NewGuid());

        var list = new List<Product> { p1, p2 };
        _productRepoMock.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .Returns(list.BuildMock());

        var command = new CreateOrderCommand(
            new List<CreateOrderItemDto> { new(p1.Id, 1), new(p2.Id, 1) },
            "C", "D", "S", "Z", "F"
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Sepetinizde farklı para birimlerine sahip ürünler var. Lütfen kontrol ediniz.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_StockIsInsufficient()
    {
        _userContextMock.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        _tenantContextMock.Setup(x => x.CompanyId).Returns(Guid.NewGuid());

        // Sadece 5 adet stok var!
        var p1 = new Product("P1", "S1", "D1", new Money(100, Currency.TRY), 5, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        SetEntityId(p1, Guid.NewGuid());

        var list = new List<Product> { p1 };
        _productRepoMock.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .Returns(list.BuildMock());

        // Kullanıcı 10 tane istiyor
        var command = new CreateOrderCommand(
            new List<CreateOrderItemDto> { new(p1.Id, 10) },
            "C", "D", "S", "Z", "F"
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("P1 için yeterli stok yok. (Mevcut: 5)");
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_DecreaseStock_And_ClearBasket_When_Valid()
    {
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);
        _tenantContextMock.Setup(x => x.CompanyId).Returns(Guid.NewGuid());

        var p1 = new Product("P1", "S1", "D1", new Money(100, Currency.TRY), 10, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        SetEntityId(p1, Guid.NewGuid());

        var list = new List<Product> { p1 };
        _productRepoMock.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .Returns(list.BuildMock());

        var command = new CreateOrderCommand(
            new List<CreateOrderItemDto> { new(p1.Id, 2) },
            "Şehir", "İlçe", "Sokak", "00000", "Tam"
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().StartWith("ORD-"); // Dönüş değeri sipariş numarası

        p1.Stock.Should().Be(8); // 10'dan 2 eksildi

        _productRepoMock.Verify(x => x.Update(p1), Times.Once);
        _orderRepoMock.Verify(x => x.Add(It.IsAny<Order>()), Times.Once);
        _basketRepoMock.Verify(x => x.DeleteBasketAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
