using ECommercePlatform.Application.Auth.Login;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Application.Tests.TestHelpers;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ECommercePlatform.Application.Tests.Auth.Login;

public class TenantLoginWithTwoFactorCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<ICompanyUserRepository> _companyUserRepositoryMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IUserRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly TenantLoginWithTwoFactorCommandHandler _handler;

    public TenantLoginWithTwoFactorCommandHandlerTests()
    {
        _userManagerMock = IdentityMocks.CreateMockUserManager<User>();
        _companyUserRepositoryMock = new Mock<ICompanyUserRepository>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _refreshTokenRepositoryMock = new Mock<IUserRefreshTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new TenantLoginWithTwoFactorCommandHandler(
            _userManagerMock.Object,
            _companyUserRepositoryMock.Object,
            _jwtProviderMock.Object,
            _refreshTokenRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    // Ortak bir kullanıcı oluşturmak için yardımcı metot
    private User CreateTestUser(string email)
    {
        var user = new User("Ufuk", "Test", email, email);
        var idProperty = typeof(IdentityUser<Guid>).GetProperty("Id");
        idProperty?.SetValue(user, Guid.NewGuid());
        return user;
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserNotFound()
    {
        // Arrange
        var command = new TenantLoginWithTwoFactorCommand("notfound@test.com", "123456", Guid.NewGuid());

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Kullanıcı bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_TwoFactorCodeIsInvalid()
    {
        // Arrange
        var command = new TenantLoginWithTwoFactorCommand("test@test.com", "wrong-code", Guid.NewGuid());
        var user = CreateTestUser(command.Email);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.VerifyTwoFactorTokenAsync(user, "SixDigit", command.Code))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Geçersiz doğrulama kodu.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CompanyUserNotFound()
    {
        // Arrange
        var command = new TenantLoginWithTwoFactorCommand("test@test.com", "123456", Guid.NewGuid());
        var user = CreateTestUser(command.Email);

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, "SixDigit", command.Code)).ReturnsAsync(true);

        // Kullanıcı var, 2FA doğru ama bu şirkete üye değil (FirstOrDefaultAsync null dönüyor)
        _companyUserRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
        It.IsAny<System.Linq.Expressions.Expression<Func<CompanyUser, bool>>>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<bool>()))
            .ReturnsAsync((CompanyUser)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu şirkete ait kullanıcı kaydı bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_When_AllValidationsPass()
    {
        // Arrange
        var command = new TenantLoginWithTwoFactorCommand("test@test.com", "123456", Guid.NewGuid());
        var user = CreateTestUser(command.Email);
        var companyUser = new CompanyUser(user.Id, command.CompanyId);

        // IdentityUser Id setlemesi gibi CompanyUser Id setlemesi (Test için gerekli olabilir)
        var entityIdProperty = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        entityIdProperty?.SetValue(companyUser, Guid.NewGuid());

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, "SixDigit", command.Code)).ReturnsAsync(true);

        _companyUserRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CompanyUser, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(companyUser);

        _jwtProviderMock
            .Setup(x => x.CreateTenantTokenAsync(user, companyUser, It.IsAny<CancellationToken>()))
            .ReturnsAsync("access-token");

        _jwtProviderMock
            .Setup(x => x.CreateRefreshToken())
            .Returns("refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("access-token");
        result.Data.RefreshToken.Should().Be("refresh-token");

        // Refresh token kaydedildi mi?
        _refreshTokenRepositoryMock.Verify(x => x.Add(It.Is<UserRefreshToken>(rt =>
            rt.UserId == user.Id &&
            rt.CompanyUserId == companyUser.Id &&
            rt.Code == "refresh-token")), Times.Once);

        // Veritabanına kaydedildi mi?
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
