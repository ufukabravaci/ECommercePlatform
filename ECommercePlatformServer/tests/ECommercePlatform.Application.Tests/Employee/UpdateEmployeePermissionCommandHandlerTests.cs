using ECommercePlatform.Application.Employee;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Employee;

public class UpdateEmployeePermissionCommandHandlerTests
{
    private readonly Mock<ICompanyUserRepository> _companyUserRepoMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateEmployeePermissionCommandHandler _handler;

    public UpdateEmployeePermissionCommandHandlerTests()
    {
        _companyUserRepoMock = new Mock<ICompanyUserRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new UpdateEmployeePermissionCommandHandler(
            _companyUserRepoMock.Object,
            _tenantContextMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CompanyUserNotFound()
    {
        _tenantContextMock.Setup(x => x.CompanyId).Returns(Guid.NewGuid());

        _companyUserRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyUser, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                            .ReturnsAsync((CompanyUser)null!);

        var command = new UpdateEmployeePermissionCommand(Guid.NewGuid(), "Orders.Read", true);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Çalışan bulunamadı veya bu şirkete ait değil.");
    }

    [Fact]
    public async Task Handle_ShouldGrantPermission_When_IsGrantedIsTrue()
    {
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var companyUser = new CompanyUser(userId, companyId);

        _companyUserRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyUser, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                            .ReturnsAsync(companyUser);

        var command = new UpdateEmployeePermissionCommand(userId, "Orders.Read", true);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Yetki başarıyla verildi.");

        companyUser.Permissions.Should().Contain("Orders.Read");

        _companyUserRepoMock.Verify(x => x.Update(companyUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRevokePermission_When_IsGrantedIsFalse()
    {
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var companyUser = new CompanyUser(userId, companyId);
        companyUser.AddPermission("Orders.Read"); // Önce yetkiyi verelim

        _companyUserRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyUser, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                            .ReturnsAsync(companyUser);

        var command = new UpdateEmployeePermissionCommand(userId, "Orders.Read", false);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Yetki başarıyla geri alındı.");

        companyUser.Permissions.Should().NotContain("Orders.Read");

        _companyUserRepoMock.Verify(x => x.Update(companyUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
