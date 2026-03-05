using ECommercePlatform.Application.Companies;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Companies;

public class UpdateShippingSettingsCommandHandlerTests
{
    private readonly Mock<IRepository<Company>> _companyRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateShippingSettingsCommandHandler _handler;

    public UpdateShippingSettingsCommandHandlerTests()
    {
        _companyRepositoryMock = new Mock<IRepository<Company>>();
        _tenantContextMock = new Mock<ITenantContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new UpdateShippingSettingsCommandHandler(
            _companyRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CompanyNotFound()
    {
        _tenantContextMock.Setup(x => x.CompanyId).Returns(Guid.NewGuid());

        _companyRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company)null!);

        var command = new UpdateShippingSettingsCommand(500, 50);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Şirket bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateSettings_When_CompanyExists()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var company = new Company("Test", "1234567890");

        _companyRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        var command = new UpdateShippingSettingsCommand(500, 50);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Kargo ayarları güncellendi.");

        company.ShippingSettings.FreeShippingThreshold.Should().Be(500);
        company.ShippingSettings.FlatRate.Should().Be(50);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
