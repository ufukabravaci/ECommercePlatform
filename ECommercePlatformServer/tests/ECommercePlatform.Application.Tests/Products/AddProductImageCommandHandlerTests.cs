using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Shared; // Money, Currency
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Http;
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Products;

public class AddProductImageCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly AddProductImageCommandHandler _handler;

    public AddProductImageCommandHandlerTests()
    {
        _productRepoMock = new Mock<IRepository<Product>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fileServiceMock = new Mock<IFileService>();
        _tenantContextMock = new Mock<ITenantContext>();

        _handler = new AddProductImageCommandHandler(
            _productRepoMock.Object,
            _unitOfWorkMock.Object,
            _fileServiceMock.Object,
            _tenantContextMock.Object
        );
    }

    private void SetEntityId(ECommercePlatform.Domain.Abstractions.Entity entity, Guid id)
    {
        var prop = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        prop?.SetValue(entity, id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_ProductNotFound()
    {
        _productRepoMock.Setup(x => x.WhereWithTracking(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .Returns(new List<Product>().BuildMock());

        var fileMock = new Mock<IFormFile>();
        var command = new AddProductImageCommand(Guid.NewGuid(), fileMock.Object, true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Ürün bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldUploadFileAndAddImageToProduct_When_Valid()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var productId = Guid.NewGuid();
        // İSMİ "P1" DEĞİL "Telefon" YAPIYORUZ
        var product = new Product("Telefon", "SKU123", "Desc", new Money(100, Currency.TRY), 10, companyId, Guid.NewGuid(), Guid.NewGuid());
        SetEntityId(product, productId);

        // MOCK AYARINI BUILD MOCK İLE GÜNCELLİYORUZ
        var mockList = new List<Product> { product }.BuildMock();

        _productRepoMock.Setup(x => x.WhereWithTracking(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                        .Returns(mockList);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns("test.jpg");
        fileMock.Setup(x => x.ContentType).Returns("image/jpeg");
        fileMock.Setup(x => x.OpenReadStream()).Returns(new MemoryStream());

        var uploadedPath = "http://localhost/uploads/test.jpg";
        _fileServiceMock.Setup(x => x.UploadAsync(It.IsAny<Stream>(), "test.jpg", "image/jpeg", $"{companyId}/products", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(uploadedPath);

        var command = new AddProductImageCommand(productId, fileMock.Object, true);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Eğer burada hala false dönüyorsa, sebebini görmek için şu satırı geçici ekleyebilirsin:
        // throw new Exception(string.Join(", ", result.ErrorMessages ?? new List<string>()));

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Resim başarıyla eklendi.");
    }

}
