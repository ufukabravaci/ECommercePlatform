using ECommercePlatform.Application.Products;
using ECommercePlatform.Domain.Brands;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Shared;
using FluentAssertions;
using GenericRepository;
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Products;

public class GetAllProductsQueryHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly GetAllProductsQueryHandler _handler;

    public GetAllProductsQueryHandlerTests()
    {
        _productRepoMock = new Mock<IRepository<Product>>();
        _handler = new GetAllProductsQueryHandler(_productRepoMock.Object);
    }

    private void SetEntityId(ECommercePlatform.Domain.Abstractions.Entity entity, Guid id)
    {
        var prop = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        prop?.SetValue(entity, id);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedAndSortedProducts()
    {
        // 1. Arrange Data
        var companyId = Guid.NewGuid();
        var cat = new Category("Kategori", companyId); SetEntityId(cat, Guid.NewGuid());
        var brand = new Brand("Marka", null, companyId); SetEntityId(brand, Guid.NewGuid());

        // Fiyatı 50 olan "Apple"
        var p1 = new Product("Apple", "SKU1", "D1", new Money(50, Currency.TRY), 10, companyId, brand.Id, cat.Id);
        // Fiyatı 100 olan "Banana"
        var p2 = new Product("Banana", "SKU2", "D2", new Money(100, Currency.TRY), 10, companyId, brand.Id, cat.Id);

        // Navigation Properties for Select Projection
        var catProp = typeof(Product).GetProperty("Category");
        var brandProp = typeof(Product).GetProperty("Brand");
        catProp?.SetValue(p1, cat); brandProp?.SetValue(p1, brand);
        catProp?.SetValue(p2, cat); brandProp?.SetValue(p2, brand);

        var list = new List<Product> { p1, p2 };
        _productRepoMock.Setup(x => x.GetAll()).Returns(list.BuildMock());

        // 2. Arama ve Sıralama Parametreleri: Name'e göre azalan (Descending) sırala
        var query = new GetAllProductsQuery(null, null, "name", "desc", 1, 10);

        // 3. Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // 4. Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(2);

        // Name'e göre Descending (B > A), yani önce Banana gelmeli
        result.Data.Items.First().Name.Should().Be("Banana");
    }
}
