using ECommercePlatform.Application.Dashboard;
using ECommercePlatform.Application.Mapping;
using ECommercePlatform.Domain.Brands;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Orders;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Reviews;
using ECommercePlatform.Domain.Shared;
using ECommercePlatform.Domain.Users;
using ECommercePlatform.Domain.Users.ValueObjects; // Address için
using FluentAssertions;
using GenericRepository;
using Mapster;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;
using System.Reflection;

namespace ECommercePlatform.Application.Tests.Dashboard;

public class GetDashboardStatsQueryHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock;
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IRepository<Category>> _categoryRepoMock;
    private readonly Mock<IRepository<Brand>> _brandRepoMock;
    private readonly Mock<IRepository<Review>> _reviewRepoMock;
    private readonly Mock<IRepository<CompanyUser>> _companyUserRepoMock;
    private readonly GetDashboardStatsQueryHandler _handler;

    public GetDashboardStatsQueryHandlerTests()
    {
        _orderRepoMock = new Mock<IRepository<Order>>();
        _productRepoMock = new Mock<IRepository<Product>>();
        _categoryRepoMock = new Mock<IRepository<Category>>();
        _brandRepoMock = new Mock<IRepository<Brand>>();
        _reviewRepoMock = new Mock<IRepository<Review>>();
        _companyUserRepoMock = new Mock<IRepository<CompanyUser>>();

        // Global Mapster Config
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        new MapsterConfig().Register(TypeAdapterConfig.GlobalSettings);

        _handler = new GetDashboardStatsQueryHandler(
            _orderRepoMock.Object,
            _productRepoMock.Object,
            _categoryRepoMock.Object,
            _brandRepoMock.Object,
            _reviewRepoMock.Object,
            _companyUserRepoMock.Object
        );
    }

    // Entity Id Ataması (Reflection)
    private void SetEntityId(ECommercePlatform.Domain.Abstractions.Entity entity, Guid id)
    {
        var prop = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        prop?.SetValue(entity, id);
    }

    private void SetUserId(User user, Guid id)
    {
        var prop = typeof(IdentityUser<Guid>).GetProperty("Id");
        prop?.SetValue(user, id);
    }

    // List<string> (Roles) ataması
    private void AddRoleToCompanyUser(CompanyUser cu, string role)
    {
        var type = typeof(CompanyUser);
        var backingField = type.GetField("_roles", BindingFlags.NonPublic | BindingFlags.Instance);
        if (backingField != null)
        {
            var list = (List<string>?)backingField.GetValue(cu);
            list?.Add(role);
        }
        else
        {
            var prop = type.GetProperty("Roles");
            var current = prop?.GetValue(cu) as ICollection<string>;
            current?.Add(role);
        }
    }

    // OrderItem Listesine eleman eklemek için
    private void AddItemToOrder(Order order, OrderItem item)
    {
        var type = typeof(Order);

        // Senin sınıfında List<OrderItem> _items; var, onu çekiyoruz.
        var backingField = type.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);

        if (backingField != null)
        {
            var list = (List<OrderItem>?)backingField.GetValue(order);

            if (list == null)
            {
                list = new List<OrderItem>();
                backingField.SetValue(order, list);
            }

            list.Add(item);
        }
        else
        {
            throw new InvalidOperationException("Order içindeki '_items' backing field bulunamadı.");
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnAccurateDashboardStats()
    {
        // ----------------- MOCK DATA HAZIRLIĞI ----------------- //
        var companyId = Guid.NewGuid();

        // 1. Categories
        var cat1 = new Category("Kategori1", companyId); SetEntityId(cat1, Guid.NewGuid());
        var categories = new List<Category> { cat1, new Category("Kategori2", companyId) };
        _categoryRepoMock.Setup(x => x.AsQueryable()).Returns(categories.BuildMock());

        // 2. Brands
        var b1 = new Brand("Brand1", null, companyId); SetEntityId(b1, Guid.NewGuid());
        var brands = new List<Brand> { b1 };
        _brandRepoMock.Setup(x => x.AsQueryable()).Returns(brands.BuildMock());

        // 3. CompanyUsers (Customers)
        var cu1 = new CompanyUser(Guid.NewGuid(), companyId); AddRoleToCompanyUser(cu1, RoleConsts.Customer);
        var cu2 = new CompanyUser(Guid.NewGuid(), companyId); AddRoleToCompanyUser(cu2, "Admin"); // Sayılmamalı
        var companyUsers = new List<CompanyUser> { cu1, cu2 };
        _companyUserRepoMock.Setup(x => x.AsQueryable()).Returns(companyUsers.BuildMock());

        // 4. Products (Low stock vs)
        var price = new Money(100, Currency.TRY);

        var p1 = new Product("P1", "SKU1", "Desc", price, 5, companyId, b1.Id, cat1.Id); // Low stock (5)
        SetEntityId(p1, Guid.NewGuid());
        p1.AddImage("p1.jpg", true); // Mapster için Main Image

        var p2 = new Product("P2", "SKU2", "Desc", price, 20, companyId, b1.Id, cat1.Id); // In stock (20)
        SetEntityId(p2, Guid.NewGuid());

        var p3 = new Product("P3", "SKU3", "Desc", price, 0, companyId, b1.Id, cat1.Id); // Out of stock (0)
        SetEntityId(p3, Guid.NewGuid());

        // Navigation properties'i Mapster için sahte (dummy) atayalım
        var catProp = typeof(Product).GetProperty("Category");
        catProp?.SetValue(p1, cat1); catProp?.SetValue(p2, cat1); catProp?.SetValue(p3, cat1);

        var brandProp = typeof(Product).GetProperty("Brand");
        brandProp?.SetValue(p1, b1); brandProp?.SetValue(p2, b1); brandProp?.SetValue(p3, b1);

        var products = new List<Product> { p1, p2, p3 };
        _productRepoMock.Setup(x => x.AsQueryable()).Returns(products.BuildMock());

        // 5. Reviews
        var review1 = new Review(p1.Id, cu1.UserId, companyId, 5, "Süper"); review1.Approve();
        var review2 = new Review(p2.Id, cu1.UserId, companyId, 3, "İdare eder"); // Pending (Default IsApproved = false)
        var reviews = new List<Review> { review1, review2 };
        _reviewRepoMock.Setup(x => x.AsQueryable()).Returns(reviews.BuildMock());

        var fakeAddress = new Address("Şehir", "İlçe", "Sokak", "00000", "Tam Adres");
        var user = new User("Test", "User", "test@test.com", "test");
        SetUserId(user, cu1.UserId);

        // Sipariş 1: Teslim edilmiş (Gelire eklenecek)
        var order1 = new Order(cu1.UserId, companyId, fakeAddress);
        SetEntityId(order1, Guid.NewGuid());

        // Senin OrderItem constructor'ına göre nesne oluşturup yardımcı metodumuzla basıyoruz.
        var orderItem1 = new OrderItem(order1.Id, p1.Id, p1.Name, price, 2);
        AddItemToOrder(order1, orderItem1);

        // Statüyü domain içindeki public metot ile güncelliyoruz (Reflection'a gerek kalmadı)
        order1.UpdateStatus(OrderStatus.Delivered);

        // Navigation property olan Customer'ı Reflection ile bağla
        var propUser = typeof(Order).GetProperty("Customer");
        propUser?.SetValue(order1, user);

        // Sipariş 2: Bekleyen (Gelire eklenmeyecek)
        var order2 = new Order(cu1.UserId, companyId, fakeAddress);
        SetEntityId(order2, Guid.NewGuid());

        var orderItem2 = new OrderItem(order2.Id, p2.Id, p2.Name, price, 1);
        AddItemToOrder(order2, orderItem2);

        order2.UpdateStatus(OrderStatus.Pending);
        propUser?.SetValue(order2, user);

        var orders = new List<Order> { order1, order2 };
        _orderRepoMock.Setup(x => x.AsQueryable()).Returns(orders.BuildMock());

        // ----------------- ACT ----------------- //
        var result = await _handler.Handle(new GetDashboardStatsQuery(), CancellationToken.None);

        // ----------------- ASSERT ----------------- //
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();

        var stats = result.Data!;

        // Kategori, Marka, Müşteri doğrulaması
        stats.TotalCategories.Should().Be(2);
        stats.TotalBrands.Should().Be(1);
        stats.TotalCustomers.Should().Be(1);

        // Ürün doğrulaması
        stats.TotalProducts.Should().Be(3);
        stats.InStockProducts.Should().Be(1);
        stats.LowStockProducts.Should().Be(1);
        stats.OutOfStockProducts.Should().Be(1);
        stats.LowStockProductsList.Should().HaveCount(1);
        stats.LowStockProductsList.First().Name.Should().Be("P1");
        stats.LowStockProductsList.First().ImageUrl.Should().Be("p1.jpg"); // Mapster Main Image çalışmış mı?

        // Sipariş Doğrulaması
        stats.TotalOrders.Should().Be(2);
        stats.DeliveredOrders.Should().Be(1);
        stats.PendingOrders.Should().Be(1);
        stats.TotalRevenue.Should().Be(200); // Sadece teslim edilenlerin (100*2) tutarı
        stats.RecentOrders.Should().HaveCount(2);
        stats.RecentOrders.First(x => x.Status == "Delivered").CustomerName.Should().Be("Test User"); // Dto mapping kontrolü

        // Yorum Doğrulaması
        stats.TotalReviews.Should().Be(2);
        stats.ApprovedReviews.Should().Be(1);
        stats.PendingReviews.Should().Be(1);
        stats.AverageRating.Should().Be(4.0); // (5+3) / 2
    }
}
