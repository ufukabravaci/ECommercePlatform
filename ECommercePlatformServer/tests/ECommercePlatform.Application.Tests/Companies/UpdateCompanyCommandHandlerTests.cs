using ECommercePlatform.Application.Companies;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Companies;

public class UpdateCompanyCommandHandlerTests
{
    private readonly Mock<ICompanyRepository> _companyRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateCompanyCommandHandler _handler;

    public UpdateCompanyCommandHandlerTests()
    {
        _companyRepositoryMock = new Mock<ICompanyRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new UpdateCompanyCommandHandler(
            _companyRepositoryMock.Object,
            _tenantContextMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_TaxNumberBelongsToAnotherCompany()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var company = new Company("Old Name", "1111111111");

        _companyRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(company);

        // Kullanıcı vergi numarasını değiştiriyor ve bu numara başkasında var
        _companyRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateCompanyCommand("New Name", "2222222222", "City", "Dist", "Street", "Zip", "Full");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu vergi numarası başka bir şirket tarafından kullanılıyor.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateCompany_When_Valid()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var company = new Company("Old Name", "1111111111");

        _companyRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(company);

        var command = new UpdateCompanyCommand("New Name", "1111111111", "İstanbul", "Kadıköy", "Moda", "34000", "Tam Adres");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();

        company.Name.Should().Be("New Name");
        company.Address.Should().NotBeNull();
        company.Address!.City.Should().Be("İstanbul");

        _companyRepositoryMock.Verify(x => x.Update(company), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
