using ECommercePlatform.Application.Products;
using ECommercePlatform.Domain.Brands;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Shared;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IRepository<Category>> _categoryRepoMock;
    private readonly Mock<IRepository<Brand>> _brandRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _productRepoMock = new Mock<IRepository<Product>>();
        _categoryRepoMock = new Mock<IRepository<Category>>();
        _brandRepoMock = new Mock<IRepository<Brand>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new UpdateProductCommandHandler(
            _productRepoMock.Object,
            _categoryRepoMock.Object,
            _unitOfWorkMock.Object,
            _brandRepoMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_ProductNotFound()
    {
        _productRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Product)null!);

        var command = new UpdateProductCommand(Guid.NewGuid(), "N", "D", 10, "TRY", 1, Guid.NewGuid(), Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldUpdateProduct_When_Valid()
    {
        var cat1 = Guid.NewGuid(); var cat2 = Guid.NewGuid();
        var brand1 = Guid.NewGuid(); var brand2 = Guid.NewGuid();

        var product = new Product("P Eski", "S", "D", new Money(10, Currency.TRY), 5, Guid.NewGuid(), brand1, cat1);

        _productRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(product);

        // Kategori ve Marka geçerliliği testleri için True dönüyoruz
        _categoryRepoMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _brandRepoMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var command = new UpdateProductCommand(product.Id, "P Yeni", "D Yeni", 50, "USD", 10, cat2, brand2);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();

        product.Name.Should().Be("P Yeni");
        product.Price.Amount.Should().Be(50);
        product.Price.Currency.Should().Be(Currency.USD);
        product.CategoryId.Should().Be(cat2);
        product.BrandId.Should().Be(brand2);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
