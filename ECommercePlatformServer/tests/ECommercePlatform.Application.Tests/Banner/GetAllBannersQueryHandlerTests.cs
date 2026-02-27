using ECommercePlatform.Application.Banners;
using ECommercePlatform.Domain.Banners;
using FluentAssertions;
using GenericRepository;
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Banners;

public class GetAllBannersQueryHandlerTests
{
    private readonly Mock<IRepository<Banner>> _bannerRepositoryMock;
    private readonly GetAllBannersQueryHandler _handler;

    public GetAllBannersQueryHandlerTests()
    {
        _bannerRepositoryMock = new Mock<IRepository<Banner>>();
        _handler = new GetAllBannersQueryHandler(_bannerRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnBanners_OrderedByOrderField()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var banner1 = new Banner("Banner 2", "Desc 2", "img2.jpg", "/target2", 2, companyId);
        var banner2 = new Banner("Banner 1", "Desc 1", "img1.jpg", "/target1", 1, companyId);

        // Veritabanında sırasız duruyorlar
        var bannersList = new List<Banner> { banner1, banner2 };

        // AsQueryable dönüyoruz.
        // Mapster'ın ProjectToType'ı IQueryable üzerinden DTO üretecek, MockQueryable (BuildMock) ToListAsync'in patlamamasını sağlayacak.
        _bannerRepositoryMock.Setup(x => x.AsQueryable()).Returns(bannersList.BuildMock());

        var query = new GetAllBannersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Count.Should().Be(2);

        // Handler içindeki OrderBy(x => x.Order) çalışmış mı? (Önce Order=1 olan gelmeli)
        result.Data.First().Title.Should().Be("Banner 1"); // Order: 1
        result.Data.Last().Title.Should().Be("Banner 2");  // Order: 2
    }
}
