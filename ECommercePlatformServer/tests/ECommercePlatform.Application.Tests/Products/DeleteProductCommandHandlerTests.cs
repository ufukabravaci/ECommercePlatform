using ECommercePlatform.Application.Products;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Shared;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Products;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _productRepoMock = new Mock<IRepository<Product>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new DeleteProductCommandHandler(
            _productRepoMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_ProductNotFound()
    {
        _productRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Product)null!);

        var command = new DeleteProductCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Ürün bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteProduct_When_Valid()
    {
        var product = new Product("P1", "S1", "D1", new Money(100, Currency.TRY), 10, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Entity içinde yazmış olduğun Delete metodunun çalışmasını test ediyoruz.
        // Product'ın base(Entity) delete metodu IsDeleted = true yapacaktır.

        _productRepoMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(product);

        var command = new DeleteProductCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Ürün başarıyla silindi.");

        product.IsDeleted.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
