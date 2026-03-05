using ECommercePlatform.Application.Companies;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using FluentAssertions;
using GenericRepository;
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Companies;

public class GetShippingSettingsQueryHandlerTests
{
    private readonly Mock<IRepository<Company>> _companyRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly GetShippingSettingsQueryHandler _handler;

    public GetShippingSettingsQueryHandlerTests()
    {
        _companyRepositoryMock = new Mock<IRepository<Company>>();
        _tenantContextMock = new Mock<ITenantContext>();
        _handler = new GetShippingSettingsQueryHandler(_companyRepositoryMock.Object, _tenantContextMock.Object);
    }

    private void SetEntityId(Company company, Guid id)
    {
        var property = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        property?.SetValue(company, id);
    }

    [Fact]
    public async Task Handle_ShouldReturnDefaultSettings_When_CompanyNotFound()
    {
        // DB'de olmayan bir CompanyId verelim
        _tenantContextMock.Setup(x => x.CompanyId).Returns(Guid.NewGuid());

        var list = new List<Company>(); // Boş tablo
        _companyRepositoryMock.Setup(x => x.AsQueryable()).Returns(list.BuildMock());

        var query = new GetShippingSettingsQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FreeShippingThreshold.Should().Be(0);
        result.Data.FlatRate.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnSettings_When_CompanyExists()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var company = new Company("Test", "1234567890");
        SetEntityId(company, companyId);

        // Entity Domain metodu ile ayarları değiştirelim
        company.UpdateShippingSettings(500, 50);

        var list = new List<Company> { company };
        _companyRepositoryMock.Setup(x => x.AsQueryable()).Returns(list.BuildMock());

        var query = new GetShippingSettingsQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FreeShippingThreshold.Should().Be(500);
        result.Data.FlatRate.Should().Be(50);
    }
}
