using ECommercePlatform.Application.Brands;
using ECommercePlatform.Domain.Brands;
using ECommercePlatform.Domain.Products;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Brands;

public class DeleteBrandCommandHandlerTests
{
    private readonly Mock<IRepository<Brand>> _brandRepositoryMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteBrandCommandHandler _handler;

    public DeleteBrandCommandHandlerTests()
    {
        _brandRepositoryMock = new Mock<IRepository<Brand>>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new DeleteBrandCommandHandler(
            _brandRepositoryMock.Object,
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_BrandHasProducts()
    {
        var command = new DeleteBrandCommand(Guid.NewGuid());
        var brand = new Brand("Test", null, Guid.NewGuid());

        _brandRepositoryMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(brand);

        // Bu markaya ait ürün var
        _productRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu markaya ait ürünler bulunduğu için silinemez.");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteBrand_When_NoProductsExist()
    {
        var command = new DeleteBrandCommand(Guid.NewGuid());
        var brand = new Brand("Test", null, Guid.NewGuid());

        _brandRepositoryMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(brand);

        _productRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(false); // Ürün yok

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        brand.IsDeleted.Should().BeTrue(); // Soft delete property'si

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
