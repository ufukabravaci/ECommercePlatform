using ECommercePlatform.Application.Auth.Register;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Application.Tests.TestHelpers;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommercePlatform.Application.Tests.Auth.Register;

public class RegisterTenantCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<ICompanyRepository> _companyRepositoryMock;
    private readonly Mock<ICompanyUserRepository> _companyUserRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _mailServiceMock;
    private readonly Mock<ILogger<RegisterTenantCommandHandler>> _loggerMock;
    private readonly RegisterTenantCommandHandler _handler;

    public RegisterTenantCommandHandlerTests()
    {
        _userManagerMock = IdentityMocks.CreateMockUserManager<User>();
        _companyRepositoryMock = new Mock<ICompanyRepository>();
        _companyUserRepositoryMock = new Mock<ICompanyUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<RegisterTenantCommandHandler>>();

        _handler = new RegisterTenantCommandHandler(
            _userManagerMock.Object,
            _companyRepositoryMock.Object,
            _companyUserRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mailServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_TaxNumberAlreadyExists()
    {
        // Arrange
        var command = ValidCommand();
        _companyRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Şirket zaten var dedirtiyoruz

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().NotBeNull();
        result.ErrorMessages.Should().ContainSingle()
            .Which.Should().Be("Bu vergi numarası ile kayıtlı şirket bulunmaktadır."); // TS.Result uyumlu kontrol

        _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserCreationFails()
    {
        // Arrange
        var command = ValidCommand();

        _companyRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((User?)null);

        // IdentityResult başarısız dönüyor
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Şifre çok zayıf." }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().NotBeNull();
        result.ErrorMessages.Should().Contain("Şifre çok zayıf."); // Identity hatası ErrorMessages listesine eklenmiş olmalı

        _companyRepositoryMock.Verify(x => x.Add(It.IsAny<Company>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_And_SendEmail_When_AllValidationsPass()
    {
        // Arrange
        var command = ValidCommand();

        _companyRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), command.Password)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<User>(), RoleConsts.CompanyOwner)).ReturnsAsync(false);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), RoleConsts.CompanyOwner)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>())).ReturnsAsync("test-token-123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Şirket kaydı başarılı. Email doğrulaması bekleniyor.");

        _companyRepositoryMock.Verify(x => x.Add(It.IsAny<Company>()), Times.Once);
        _companyUserRepositoryMock.Verify(x => x.Add(It.IsAny<CompanyUser>()), Times.Once);

        // 1. Company eklendikten sonra, 2. CompanyUser eklendikten sonra SaveChangesAsync çağrılıyor.
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));

        // Mail gönderimi tetiklendi mi?
        _mailServiceMock.Verify(x => x.SendAsync(
            command.Email,
            It.Is<string>(subject => subject.Contains("Kayıt Onayı")),
            It.Is<string>(body => body.Contains("test-token-123")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // Ortak valid data oluşturucu
    private static RegisterTenantCommand ValidCommand() => new(
        FirstName: "Ufuk",
        LastName: "Test",
        Email: "ufuk@test.com",
        Password: "Password123*",
        ConfirmPassword: "Password123*",
        CompanyName: "Ufuk Yazılım",
        TaxNumber: "1234567890",
        TaxOffice: "Besiktas",
        FullAddress: "Barbaros Bulvarı No:1",
        City: "Istanbul",
        District: "Besiktas",
        Street: "Barbaros",
        ZipCode: "34353"
    );
}
