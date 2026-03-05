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

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _productRepoMock = new Mock<IRepository<Product>>();
        _handler = new GetProductByIdQueryHandler(_productRepoMock.Object);
    }

    private void SetEntityId(ECommercePlatform.Domain.Abstractions.Entity entity, Guid id)
    {
        var prop = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        prop?.SetValue(entity, id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_ProductNotFound()
    {
        var list = new List<Product>();
        _productRepoMock.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                        .Returns(list.BuildMock());

        var result = await _handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Ürün bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnProduct_When_Exists()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var cat = new Category("Kategori", companyId); SetEntityId(cat, Guid.NewGuid());
        var brand = new Brand("Marka", null, companyId); SetEntityId(brand, Guid.NewGuid());

        var product = new Product("P1", "SKU1", "D1", new Money(100, Currency.TRY), 10, companyId, brand.Id, cat.Id);
        SetEntityId(product, productId);
        product.AddImage("test.jpg", true);

        var catProp = typeof(Product).GetProperty("Category");
        var brandProp = typeof(Product).GetProperty("Brand");
        catProp?.SetValue(product, cat); brandProp?.SetValue(product, brand);

        var list = new List<Product> { product };
        _productRepoMock.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                        .Returns(list.BuildMock()); // IQueryable Mocklanıyor çünkü handler Select yapıyor

        var result = await _handler.Handle(new GetProductByIdQuery(productId), CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("P1");
        result.Data.MainImageUrl.Should().Be("test.jpg");
    }
}
