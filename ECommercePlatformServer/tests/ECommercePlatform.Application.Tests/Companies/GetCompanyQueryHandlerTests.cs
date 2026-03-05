using ECommercePlatform.Application.Companies;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Moq;

namespace ECommercePlatform.Application.Tests.Companies;

public class GetCompanyQueryHandlerTests
{
    private readonly Mock<ICompanyRepository> _companyRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly IMapper _mapper; // Gerçek mapper kullanacağız
    private readonly GetCompanyQueryHandler _handler;

    public GetCompanyQueryHandlerTests()
    {
        _companyRepositoryMock = new Mock<ICompanyRepository>();
        _tenantContextMock = new Mock<ITenantContext>();

        // Mapster'ın Gerçek Kurulumu (Mock değil)
        var config = new TypeAdapterConfig();
        config.NewConfig<Company, CompanyDto>()
            .Map(dest => dest.City, src => src.Address != null ? src.Address.City : "")
            .Map(dest => dest.District, src => src.Address != null ? src.Address.District : "")
            .Map(dest => dest.Street, src => src.Address != null ? src.Address.Street : "")
            .Map(dest => dest.FullAddress, src => src.Address != null ? src.Address.FullAddress : "");

        _mapper = new Mapper(config); // Gerçek mapper objesi oluşturuldu

        _handler = new GetCompanyQueryHandler(
            _companyRepositoryMock.Object,
            _tenantContextMock.Object,
            _mapper
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CompanyIdIsNullInContext()
    {
        _tenantContextMock.Setup(x => x.CompanyId).Returns((Guid?)null);

        var query = new GetCompanyQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Herhangi bir şirkete bağlı değilsiniz.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CompanyNotFound()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        _companyRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>(), false))
            .ReturnsAsync((Company)null!);

        var query = new GetCompanyQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Şirket bilgisi bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnCompanyDto_WithEmptyAddress_When_AddressIsNull()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var company = new Company("Test Company", "1234567890"); // Adres null

        _companyRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(company);

        var query = new GetCompanyQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test Company");

        // Mapster kuralları sayesinde null patlaması yaşanmamalı ve boş string gelmeli
        result.Data.City.Should().Be("");
        result.Data.District.Should().Be("");
    }

    [Fact]
    public async Task Handle_ShouldReturnCompanyDto_WithFullAddress_When_AddressIsNotNull()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var company = new Company("Test Company", "1234567890");
        var address = new Address("İstanbul", "Kadıköy", "Moda", "34000", "türkiye"); // Projendeki ValueObject parametrelerine göre düzenle
        company.UpdateAddress(address);

        _companyRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(company);

        var query = new GetCompanyQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Mapster adresi doğru haritalamış mı?
        result.Data!.City.Should().Be("İstanbul");
        result.Data.District.Should().Be("Kadıköy");
    }
}
