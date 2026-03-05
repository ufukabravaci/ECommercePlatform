using ECommercePlatform.Application.Reviews;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Reviews;
using ECommercePlatform.Domain.Shared; // Money vb.
using ECommercePlatform.Domain.Users.ValueObjects; // Address
using FluentAssertions;
using GenericRepository;
using MockQueryable;
using Moq;
using System.Reflection;

namespace ECommercePlatform.Application.Tests.Reviews;

public class CreateReviewCommandHandlerTests
{
    private readonly Mock<IRepository<Review>> _reviewRepoMock;
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IRepository<Order>> _orderRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly CreateReviewCommandHandler _handler;

    public CreateReviewCommandHandlerTests()
    {
        _reviewRepoMock = new Mock<IRepository<Review>>();
        _productRepoMock = new Mock<IRepository<Product>>();
        _orderRepoMock = new Mock<IRepository<Order>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userContextMock = new Mock<IUserContext>();

        _handler = new CreateReviewCommandHandler(
            _reviewRepoMock.Object,
            _productRepoMock.Object,
            _orderRepoMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object
        );
    }

    private void AddItemToOrder(Order order, OrderItem item)
    {
        var type = typeof(Order);
        var backingField = type.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<OrderItem>?)backingField?.GetValue(order);
        list?.Add(item);
    }

    private void SetEntityId(ECommercePlatform.Domain.Abstractions.Entity entity, Guid id)
    {
        var prop = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        prop?.SetValue(entity, id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_ProductNotFound()
    {
        _userContextMock.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        _productRepoMock.Setup(x => x.AsQueryable()).Returns(new List<Product>().BuildMock());

        var command = new CreateReviewCommand(Guid.NewGuid(), 5, "Harika");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Ürün bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserHasNotPurchasedProduct()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        var product = new Product("Telefon", "SKU", "D", new Money(100, Currency.TRY), 10, companyId, Guid.NewGuid(), Guid.NewGuid());
        SetEntityId(product, productId);

        _productRepoMock.Setup(x => x.AsQueryable()).Returns(new List<Product> { product }.BuildMock());

        // Kullanıcının sipariş listesi BOŞ
        _orderRepoMock.Setup(x => x.AsQueryable()).Returns(new List<Order>().BuildMock());

        var command = new CreateReviewCommand(productId, 5, "Harika");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu ürüne yorum yapmak için önce satın almalısınız.");
    }

    [Fact]
    public async Task Handle_ShouldCreateReview_When_Valid()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        var product = new Product("Telefon", "SKU", "D", new Money(100, Currency.TRY), 10, companyId, Guid.NewGuid(), Guid.NewGuid());
        SetEntityId(product, productId);

        var order = new Order(userId, companyId, new Address("C", "D", "S", "Z", "F"));
        SetEntityId(order, Guid.NewGuid());

        var orderItem = new OrderItem(order.Id, productId, "Telefon", new Money(100, Currency.TRY), 1);
        AddItemToOrder(order, orderItem);

        _productRepoMock.Setup(x => x.AsQueryable()).Returns(new List<Product> { product }.BuildMock());
        _orderRepoMock.Setup(x => x.AsQueryable()).Returns(new List<Order> { order }.BuildMock());

        var command = new CreateReviewCommand(productId, 5, "Harika");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        _reviewRepoMock.Verify(x => x.Add(It.Is<Review>(r => r.Rating == 5 && r.Comment == "Harika" && r.CustomerId == userId)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
