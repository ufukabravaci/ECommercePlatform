using ECommercePlatform.Application.Auth.Register;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Application.Tests.TestHelpers;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ECommercePlatform.Application.Tests.Auth.Register;

public class RegisterCustomerCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<ICompanyUserRepository> _companyUserRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterCustomerCommandHandler _handler;

    public RegisterCustomerCommandHandlerTests()
    {
        _userManagerMock = IdentityMocks.CreateMockUserManager<User>();
        _companyUserRepositoryMock = new Mock<ICompanyUserRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RegisterCustomerCommandHandler(
            _userManagerMock.Object,
            _companyUserRepositoryMock.Object,
            _tenantContextMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_TenantIdIsNull()
    {
        // Arrange
        _tenantContextMock.Setup(x => x.CompanyId).Returns((Guid?)null); // Mağaza bilgisi yok

        // Act
        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Mağaza bilgisi bulunamadı (Tenant ID eksik).");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserExistsAndAlreadyMemberOfCompany()
    {
        // Arrange
        var command = ValidCommand();
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var existingUser = new User("Ahmet", "Yılmaz", command.Email, command.Email);
        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(existingUser);

        // Zaten bu mağazanın üyesi
        _companyUserRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu e-posta adresi ile bu mağazada zaten bir kayıt mevcut. Lütfen giriş yapınız.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserExistsButNotMemberOfCompany()
    {
        // Arrange
        var command = ValidCommand();
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var existingUser = new User("Ahmet", "Yılmaz", command.Email, command.Email);
        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(existingUser);

        // Bu mağazanın üyesi DEĞİL
        _companyUserRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu e-posta adresi sistemde kayıtlı. Lütfen giriş yapınız.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_When_UserIsNewAndCreationSucceeds()
    {
        // Arrange
        var command = ValidCommand();
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        // Globalde böyle bir kullanıcı yok
        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), command.Password)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Üyelik kaydı başarıyla oluşturuldu.");

        // Repository'e şirket kaydı (CompanyUser) eklendi mi?
        _companyUserRepositoryMock.Verify(x => x.Add(It.Is<CompanyUser>(cu =>
            cu.CompanyId == companyId &&
            cu.Roles.Contains(RoleConsts.Customer))), Times.Once);

        // Değişiklikler kaydedildi mi?
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static RegisterCustomerCommand ValidCommand() => new(
        FirstName: "Ahmet",
        LastName: "Yılmaz",
        Email: "ahmet@test.com",
        Password: "Password123",
        ConfirmPassword: "Password123"
    );
}
