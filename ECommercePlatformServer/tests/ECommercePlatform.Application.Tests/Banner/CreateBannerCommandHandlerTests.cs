using ECommercePlatform.Application.Banners;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Banners;
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ECommercePlatform.Application.Tests.Banners;

public class CreateBannerCommandHandlerTests
{
    private readonly Mock<IRepository<Banner>> _bannerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly CreateBannerCommandHandler _handler;

    public CreateBannerCommandHandlerTests()
    {
        _bannerRepositoryMock = new Mock<IRepository<Banner>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _fileServiceMock = new Mock<IFileService>();

        _handler = new CreateBannerCommandHandler(
            _bannerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _fileServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUploadFile_And_SaveBanner_When_CommandIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        // IFormFile Mock Ayarları (Gelişmiş Mocklama)
        var fileMock = new Mock<IFormFile>();
        var content = "Fake Image Content";
        var fileName = "test-banner.jpg";
        var contentType = "image/jpeg";
        var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)); // Sahte stream yarat

        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);

        var command = new CreateBannerCommand(
            "Yaz İndirimi",
            "Mükemmel fırsatlar",
            fileMock.Object,
            "/kampanyalar",
            1);

        var expectedUploadedUrl = "https://cdn.test.com/uploads/banners/123/test-banner.jpg";

        // FileService mock'unun doğru parametrelerle çağrılıp çağrılmadığını kontrol edeceğiz ve URL döneceğiz
        _fileServiceMock
            .Setup(x => x.UploadAsync(
                It.IsAny<Stream>(),
                fileName,
                contentType,
                $"banners/{companyId}",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUploadedUrl);

        Banner? addedBanner = null;

        // Repository'ye Add çağrıldığında parametreyi yakalamak için Callback kullanıyoruz
        _bannerRepositoryMock
            .Setup(x => x.Add(It.IsAny<Banner>()))
            .Callback<Banner>(b => addedBanner = b);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeEmpty(); // Guid dönmüş mü?

        // Veritabanına kayıt işlemi doğru objeyle yapıldı mı?
        addedBanner.Should().NotBeNull();
        addedBanner!.Title.Should().Be("Yaz İndirimi");
        addedBanner.ImageUrl.Should().Be(expectedUploadedUrl);
        addedBanner.CompanyId.Should().Be(companyId);

        // UnitOfWork save çağrıldı mı?
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Stream kapatılmalı (using bloğu olduğu için)
        // MemoryStream dispose edilebilir, ancak IFormFile üzerinden test edemeyiz, 
        // FileService'in çağrıldığını garantilemek yeterlidir.
        _fileServiceMock.Verify(x => x.UploadAsync(It.IsAny<Stream>(), fileName, contentType, $"banners/{companyId}", It.IsAny<CancellationToken>()), Times.Once);
    }
}
