using ECommercePlatform.Application.Banners;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Banners;
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ECommercePlatform.Application.Tests.Banners;

public class UpdateBannerCommandHandlerTests
{
    private readonly Mock<IRepository<Banner>> _bannerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly UpdateBannerCommandHandler _handler;

    public UpdateBannerCommandHandlerTests()
    {
        _bannerRepositoryMock = new Mock<IRepository<Banner>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fileServiceMock = new Mock<IFileService>();

        _handler = new UpdateBannerCommandHandler(
            _bannerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_BannerNotFound()
    {
        var command = new UpdateBannerCommand(Guid.NewGuid(), "Title", "Desc", null, "/url", 1);

        _bannerRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Banner, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Banner)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Banner bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateTextOnly_When_ImageIsNull()
    {
        var bannerId = Guid.NewGuid();
        var banner = new Banner("Old Title", "Old Desc", "old-image.jpg", "/old-url", 1, Guid.NewGuid());

        var command = new UpdateBannerCommand(bannerId, "New Title", "New Desc", null, "/new-url", 2);

        _bannerRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Banner, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(banner);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();

        // Entity Domain metodu çalışmış mı?
        banner.Title.Should().Be("New Title");
        banner.TargetUrl.Should().Be("/new-url");
        banner.Order.Should().Be(2);
        banner.ImageUrl.Should().Be("old-image.jpg"); // Image değişmemiş olmalı!

        // FileService hiç çağrılmamalı
        _fileServiceMock.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
        _fileServiceMock.Verify(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDeleteOldImage_And_UploadNewImage_When_ImageIsProvided()
    {
        var bannerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var banner = new Banner("Old Title", "Old Desc", "http://test.com/uploads/old.jpg", "/old", 1, companyId);

        // Yeni resim Mock
        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Fake Image"));
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.FileName).Returns("new.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        var command = new UpdateBannerCommand(bannerId, "New Title", "New Desc", fileMock.Object, "/new", 2);
        var expectedUploadedUrl = "http://test.com/uploads/new.jpg";

        _bannerRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Banner, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(banner);

        _fileServiceMock
            .Setup(x => x.UploadAsync(It.IsAny<Stream>(), "new.jpg", "image/jpeg", $"banners/{companyId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUploadedUrl);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();

        // Entity Domain metodu resmi de güncellemiş mi?
        banner.ImageUrl.Should().Be(expectedUploadedUrl);

        // Eski resim silme metodu çağrılmış mı? (Uri ile AbsolutePath ayrıştıran kısım)
        _fileServiceMock.Verify(x => x.Delete("/uploads/old.jpg"), Times.Once);

        // Yeni resim yükleme metodu çağrılmış mı?
        _fileServiceMock.Verify(x => x.UploadAsync(It.IsAny<Stream>(), "new.jpg", "image/jpeg", $"banners/{companyId}", It.IsAny<CancellationToken>()), Times.Once);
    }
}
