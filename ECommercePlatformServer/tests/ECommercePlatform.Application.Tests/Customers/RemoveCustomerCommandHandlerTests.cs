using ECommercePlatform.Application.Customers;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Customers;

public class RemoveCustomerCommandHandlerTests
{
    private readonly Mock<IRepository<CompanyUser>> _companyUserRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly RemoveCustomerCommandHandler _handler;

    public RemoveCustomerCommandHandlerTests()
    {
        _companyUserRepositoryMock = new Mock<IRepository<CompanyUser>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();

        _handler = new RemoveCustomerCommandHandler(
            _companyUserRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CustomerNotFound()
    {
        var command = new RemoveCustomerCommand(Guid.NewGuid());
        _tenantContextMock.Setup(x => x.CompanyId).Returns(Guid.NewGuid());

        _companyUserRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyUser)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Müşteri bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldDeleteCompanyUser_When_Found()
    {
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var command = new RemoveCustomerCommand(customerId);

        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var companyUser = new CompanyUser(customerId, companyId);

        _companyUserRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(companyUser);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Müşteri şirketinizden çıkarıldı.");

        _companyUserRepositoryMock.Verify(x => x.Delete(companyUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
