using ECommercePlatform.Application.Mapping; // MapsterConfig için
using ECommercePlatform.Application.Reviews;
using ECommercePlatform.Domain.Reviews;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using GenericRepository;
using Mapster;
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Reviews;

public class GetProductReviewsQueryHandlerTests
{
    private readonly Mock<IRepository<Review>> _reviewRepoMock;
    private readonly GetProductReviewsQueryHandler _handler;

    public GetProductReviewsQueryHandlerTests()
    {
        _reviewRepoMock = new Mock<IRepository<Review>>();

        // Mapster Configuration
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        new MapsterConfig().Register(TypeAdapterConfig.GlobalSettings);

        _handler = new GetProductReviewsQueryHandler(_reviewRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyApprovedReviewsForProduct()
    {
        var productId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var r1 = new Review(productId, Guid.NewGuid(), companyId, 5, "Onaylı Yorum");
        r1.Approve(); // IsApproved = true

        var r2 = new Review(productId, Guid.NewGuid(), companyId, 1, "Onaysız Yorum");
        // Varsayılan IsApproved = false

        var r3 = new Review(Guid.NewGuid(), Guid.NewGuid(), companyId, 4, "Başka Ürünün Yorumu");
        r3.Approve();

        // Customer navigation property'sini doldur (Mapster string birleştirme yapacak)
        var user = new User("Ahmet", "Yılmaz", "test@test.com", "test");
        var prop = typeof(Review).GetProperty("Customer");
        prop?.SetValue(r1, user);
        prop?.SetValue(r2, user);
        prop?.SetValue(r3, user);

        var list = new List<Review> { r1, r2, r3 };
        _reviewRepoMock.Setup(x => x.AsQueryable()).Returns(list.BuildMock());

        var query = new GetProductReviewsQuery(productId, 1, 10);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Sadece r1 dönmeli! (r2 onaysız, r3 başka ürün)
        result.Data!.TotalCount.Should().Be(1);
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Comment.Should().Be("Onaylı Yorum");
        result.Data.Items.First().CustomerName.Should().Be("Ahmet Yılmaz");
    }
}
