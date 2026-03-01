using ECommercePlatform.Application.Brands;
using ECommercePlatform.Domain.Brands;
using FluentAssertions;
using GenericRepository;
using MockQueryable; // BuildMock için
using Moq;

namespace ECommercePlatform.Application.Tests.Brands;

public class GetAllBrandsQueryHandlerTests
{
    private readonly Mock<IRepository<Brand>> _brandRepositoryMock;
    private readonly GetAllBrandsQueryHandler _handler;

    public GetAllBrandsQueryHandlerTests()
    {
        _brandRepositoryMock = new Mock<IRepository<Brand>>();
        _handler = new GetAllBrandsQueryHandler(_brandRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResults_And_FilterBySearch()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var brand1 = new Brand("Apple", null, companyId);
        var brand2 = new Brand("Samsung", null, companyId);
        var brand3 = new Brand("Asus", null, companyId);
        var list = new List<Brand> { brand1, brand2, brand3 };

        _brandRepositoryMock.Setup(x => x.AsQueryable()).Returns(list.BuildMock());

        // Arama kelimesi "Ap", PageSize 2
        var query = new GetAllBrandsQuery(1, 2, "Ap");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(1); // Sadece Apple eşleşiyor
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Name.Should().Be("Apple");
    }
}
