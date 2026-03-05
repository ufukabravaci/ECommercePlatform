using ECommercePlatform.Application.Products;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Brands;
using ECommercePlatform.Domain.Products;
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ECommercePlatform.Application.Tests.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IRepository<Brand>> _brandRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _productRepoMock = new Mock<IRepository<Product>>();
        _brandRepoMock = new Mock<IRepository<Brand>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fileServiceMock = new Mock<IFileService>();
        _tenantContextMock = new Mock<ITenantContext>();

        _handler = new CreateProductCommandHandler(
            _productRepoMock.Object,
            _brandRepoMock.Object,
            _unitOfWorkMock.Object,
            _fileServiceMock.Object,
            _tenantContextMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_SkuAlreadyExists()
    {
        _tenantContextMock.Setup(x => x.CompanyId).Returns(Guid.NewGuid());

        _productRepoMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true); // SKU Zaten Var

        var command = new CreateProductCommand("Name", "SKU1", "Desc", 100, "TRY", 10, Guid.NewGuid(), Guid.NewGuid(), null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu SKU zaten kullanımda.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_BrandIsInvalid()
    {
        _tenantContextMock.Setup(x => x.CompanyId).Returns(Guid.NewGuid());

        _productRepoMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false); // Sku OK

        // Marka yok
        _brandRepoMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

        var command = new CreateProductCommand("Name", "SKU1", "Desc", 100, "TRY", 10, Guid.NewGuid(), Guid.NewGuid(), null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Geçersiz marka seçimi.");
    }

    [Fact]
    public async Task Handle_ShouldCreateProduct_And_UploadFiles_When_Valid()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        _productRepoMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

        _brandRepoMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

        // Sahte Dosya (Mock IFormFileCollection)
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        var fileCollectionMock = new Mock<IFormFileCollection>();
        fileCollectionMock.Setup(fc => fc.GetEnumerator()).Returns(new List<IFormFile> { fileMock.Object }.GetEnumerator());

        _fileServiceMock.Setup(x => x.UploadAsync(It.IsAny<Stream>(), "test.jpg", "image/jpeg", $"{companyId}/products", It.IsAny<CancellationToken>()))
                        .ReturnsAsync("uploaded_path.jpg");

        var command = new CreateProductCommand("P1", "SKU1", "Desc", 100, "TRY", 10, Guid.NewGuid(), Guid.NewGuid(), fileCollectionMock.Object);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();

        _productRepoMock.Verify(x => x.AddAsync(It.Is<Product>(p => p.Name == "P1" && p.Sku == "SKU1"), It.IsAny<CancellationToken>()), Times.Once);
        _fileServiceMock.Verify(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
