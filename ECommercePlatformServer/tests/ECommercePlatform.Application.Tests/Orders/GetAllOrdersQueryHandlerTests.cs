using ECommercePlatform.Application.Mapping; // MapsterConfig sınıfı için
using ECommercePlatform.Application.Orders;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Users;
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentAssertions;
using GenericRepository;
using Mapster;
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Orders;

public class GetAllOrdersQueryHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock;
    private readonly GetAllOrdersQueryHandler _handler;

    public GetAllOrdersQueryHandlerTests()
    {
        _orderRepoMock = new Mock<IRepository<Order>>();

        // Global Mapster Config
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        new MapsterConfig().Register(TypeAdapterConfig.GlobalSettings);

        _handler = new GetAllOrdersQueryHandler(_orderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedOrderList_And_SupportSearch()
    {
        var companyId = Guid.NewGuid();
        var fakeAddress = new Address("C", "D", "S", "Z", "F");

        var o1 = new Order(Guid.NewGuid(), companyId, fakeAddress);
        var o2 = new Order(Guid.NewGuid(), companyId, fakeAddress);

        // Mapster'ın Dto çevirimi için Customer (User) property'sini doldurmalıyız
        var user = new User("Ahmet", "Yılmaz", "test@test.com", "test");
        var userProp = typeof(Order).GetProperty("Customer");
        userProp?.SetValue(o1, user);
        userProp?.SetValue(o2, user);

        var list = new List<Order> { o1, o2 };
        _orderRepoMock.Setup(x => x.AsQueryable()).Returns(list.BuildMock());

        // Kullanıcı o1'in sipariş numarasını arıyor
        var query = new GetAllOrdersQuery(1, 10, o1.OrderNumber);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.TotalCount.Should().Be(1);
        result.Data.Items.Should().HaveCount(1);

        // Mapster CustomerName string birleştirmesi çalışmış mı? (FirstName + " " + LastName)
        result.Data.Items.First().CustomerName.Should().Be("Ahmet Yılmaz");
    }
}
