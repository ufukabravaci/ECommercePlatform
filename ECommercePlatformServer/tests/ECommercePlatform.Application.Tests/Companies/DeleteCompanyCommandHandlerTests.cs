using ECommercePlatform.Application.Companies;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Companies;

public class DeleteCompanyCommandHandlerTests
{
    private readonly Mock<ICompanyRepository> _companyRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteCompanyCommandHandler _handler;

    public DeleteCompanyCommandHandlerTests()
    {
        _companyRepositoryMock = new Mock<ICompanyRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new DeleteCompanyCommandHandler(
            _companyRepositoryMock.Object,
            _tenantContextMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CompanyIdIsNullInContext()
    {
        _tenantContextMock.Setup(x => x.CompanyId).Returns((Guid?)null);

        var command = new DeleteCompanyCommand();
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Şirket bulunamadı.");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CompanyNotFoundInDatabase()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        _companyRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync((Company)null!);

        var command = new DeleteCompanyCommand();
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Şirket bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteCompany_And_ReturnSuccess_When_Valid()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var company = new Company("Test Company", "1234567890");

        _companyRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(company);

        var command = new DeleteCompanyCommand();
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Şirket hesabı başarıyla kapatıldı (Silindi).");

        // Entity base'inden gelen IsDeleted özelliğinin true olmasını bekliyoruz.
        company.IsDeleted.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
