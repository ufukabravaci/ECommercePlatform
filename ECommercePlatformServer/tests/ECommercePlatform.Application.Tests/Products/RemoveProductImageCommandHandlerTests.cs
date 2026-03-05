using ECommercePlatform.Application.Products;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Shared;
using FluentAssertions;
using GenericRepository;
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Products;

public class RemoveProductImageCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly RemoveProductImageCommandHandler _handler;

    public RemoveProductImageCommandHandlerTests()
    {
        _productRepoMock = new Mock<IRepository<Product>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fileServiceMock = new Mock<IFileService>();

        _handler = new RemoveProductImageCommandHandler(
            _productRepoMock.Object,
            _unitOfWorkMock.Object,
            _fileServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_ImageNotFound()
    {
        var product = new Product("Telefon", "SKU123", "Açıklama", new Money(10, Currency.TRY), 1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        // Hiç resim eklenmedi

        _productRepoMock.Setup(x => x.WhereWithTracking(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                    .Returns(new List<Product> { product }.BuildMock());

        var command = new RemoveProductImageCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Silinecek resim bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldRemoveImage_And_DeleteFile_When_Valid()
    {
        var product = new Product("Telefon", "SKU123", "Açıklama", new Money(10, Currency.TRY), 1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        product.AddImage("test.jpg", true);

        var imageId = product.Images.First().Id; // Yeni eklenen resmin ID'si

        _productRepoMock.Setup(x => x.WhereWithTracking(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .Returns(new List<Product> { product }.BuildMock());

        var command = new RemoveProductImageCommand(Guid.NewGuid(), imageId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Resim başarıyla silindi.");

        // Dosya diskten (servis üzerinden) silinmeli
        _fileServiceMock.Verify(x => x.Delete("test.jpg"), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
