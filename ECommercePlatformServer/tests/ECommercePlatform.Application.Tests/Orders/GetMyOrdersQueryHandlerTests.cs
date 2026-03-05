using ECommercePlatform.Application.Mapping;
using ECommercePlatform.Application.Orders;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Users;
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentAssertions;
using GenericRepository;
using Mapster;
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Orders;

public class GetMyOrdersQueryHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly GetMyOrdersQueryHandler _handler;

    public GetMyOrdersQueryHandlerTests()
    {
        _orderRepoMock = new Mock<IRepository<Order>>();
        _userContextMock = new Mock<IUserContext>();

        // Mapster Configuration
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        new MapsterConfig().Register(TypeAdapterConfig.GlobalSettings);

        _handler = new GetMyOrdersQueryHandler(_orderRepoMock.Object, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyCurrentUserOrders()
    {
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _userContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);

        var fakeAddress = new Address("C", "D", "S", "Z", "F");

        // currentUser'ın siparişi
        var o1 = new Order(currentUserId, companyId, fakeAddress);
        // Başka kullanıcının siparişi
        var o2 = new Order(otherUserId, companyId, fakeAddress);

        // Mapster'ın Dto çevirimi için Customer'ı doldur
        var u1 = new User("U1", "L1", "u1@test.com", "t");
        var u2 = new User("U2", "L2", "u2@test.com", "t");
        typeof(Order).GetProperty("Customer")?.SetValue(o1, u1);
        typeof(Order).GetProperty("Customer")?.SetValue(o2, u2);

        var list = new List<Order> { o1, o2 };
        _orderRepoMock.Setup(x => x.AsQueryable()).Returns(list.BuildMock());

        var query = new GetMyOrdersQuery(1, 10);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Sistemde 2 sipariş var ama currentUser'ın sadece 1 siparişi var
        result.Data!.TotalCount.Should().Be(1);
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().CustomerName.Should().Be("U1 L1");
    }
}
