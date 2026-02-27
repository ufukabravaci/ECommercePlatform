using ECommercePlatform.Application.Banners;
using ECommercePlatform.Domain.Banners;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Banners;

public class DeleteBannerCommandHandlerTests
{
    private readonly Mock<IRepository<Banner>> _bannerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteBannerCommandHandler _handler;

    public DeleteBannerCommandHandlerTests()
    {
        _bannerRepositoryMock = new Mock<IRepository<Banner>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new DeleteBannerCommandHandler(
            _bannerRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_BannerNotFound()
    {
        var command = new DeleteBannerCommand(Guid.NewGuid());

        _bannerRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Banner, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Banner)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Banner bulunamadı.");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteBanner_And_ReturnSuccess_When_BannerExists()
    {
        var bannerId = Guid.NewGuid();
        var command = new DeleteBannerCommand(bannerId);

        var banner = new Banner("Title", "Desc", "url", "/target", 1, Guid.NewGuid());
        // Entity IsDeleted property'si base'den geliyor.

        _bannerRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Banner, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(banner);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Banner silindi.");

        // Banner.Delete() metodu çalışmış mı? (Soft delete)
        banner.IsDeleted.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
