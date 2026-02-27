using ECommercePlatform.Application.Auth.Login;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Application.Tests.TestHelpers;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Auth.Login;

public class TenantLoginCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IEmailService> _mailServiceMock;
    private readonly Mock<IUserRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ICompanyUserRepository> _companyUserRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly TenantLoginCommandHandler _handler;

    public TenantLoginCommandHandlerTests()
    {
        _userManagerMock = IdentityMocks.CreateMockUserManager<User>();
        // IJwtProvider sendeki projeye özel bir arayüz, mockunu boş oluşturuyoruz
        _jwtProviderMock = new Mock<IJwtProvider>();
        _mailServiceMock = new Mock<IEmailService>();
        _refreshTokenRepositoryMock = new Mock<IUserRefreshTokenRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _companyUserRepositoryMock = new Mock<ICompanyUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new TenantLoginCommandHandler(
            _userManagerMock.Object,
            _jwtProviderMock.Object,
            _mailServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            _tenantContextMock.Object,
            _companyUserRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    // Ortak bir kullanıcı oluşturmak için yardımcı metot (Id'yi reflection ile setliyoruz çünkü Entity'de private set)
    private User CreateTestUser()
    {
        var user = new User("Ufuk", "Test", "ufuk@test.com", "ufuktest");
        var idProperty = typeof(IdentityUser<Guid>).GetProperty("Id");
        idProperty?.SetValue(user, Guid.NewGuid());
        return user;
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserNotFound()
    {
        var command = new TenantLoginCommand("wrong@test.com", "Password123");
        _userManagerMock.Setup(x => x.FindByEmailAsync(command.EmailOrUserName)).ReturnsAsync((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Kullanıcı adı veya şifre hatalı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_PasswordIsIncorrect()
    {
        var command = new TenantLoginCommand("ufuk@test.com", "WrongPassword");
        var user = CreateTestUser();

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.EmailOrUserName)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Kullanıcı adı veya şifre hatalı.");
        _userManagerMock.Verify(x => x.AccessFailedAsync(user), Times.Once); // Başarısız deneme loglandı mı?
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserHasNoCompaniesAndNoTenantHeader()
    {
        var command = new TenantLoginCommand("ufuk@test.com", "CorrectPassword");
        var user = CreateTestUser();

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.EmailOrUserName)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password)).ReturnsAsync(true);
        _tenantContextMock.Setup(x => x.CompanyId).Returns((Guid?)null); // Header'da ID yok

        // Şirketi yok (Boş liste mocklanıyor)
        var emptyCompanyList = new List<CompanyUser>();
        _companyUserRepositoryMock.Setup(x => x.GetAll()).Returns(emptyCompanyList.BuildMock());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bağlı olduğunuz bir şirket bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserHasMultipleCompaniesAndNoTenantHeader()
    {
        var command = new TenantLoginCommand("ufuk@test.com", "CorrectPassword");
        var user = CreateTestUser();

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.EmailOrUserName)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password)).ReturnsAsync(true);
        _tenantContextMock.Setup(x => x.CompanyId).Returns((Guid?)null);

        // İki farklı şirketi var
        var companyList = new List<CompanyUser>
        {
            new CompanyUser(user.Id, Guid.NewGuid()),
            new CompanyUser(user.Id, Guid.NewGuid())
        };
        _companyUserRepositoryMock.Setup(x => x.GetAll()).Returns(companyList.BuildMock());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Birden fazla şirketiniz var. Lütfen giriş yapılacak şirketi seçiniz.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessAndTokens_When_EverythingIsValid()
    {
        // Arrange
        var command = new TenantLoginCommand("ufuk@test.com", "CorrectPassword");
        var user = CreateTestUser();
        var targetCompanyId = Guid.NewGuid();
        var companyUser = new CompanyUser(user.Id, targetCompanyId);

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.EmailOrUserName)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password)).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);

        // Header'dan CompanyId geliyor (Akıllı Logic A senaryosu)
        _tenantContextMock.Setup(x => x.CompanyId).Returns(targetCompanyId);

        // 4. Adım: Yetki kontrolü mock'u (FirstOrDefautAsync)
        _companyUserRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CompanyUser, bool>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<bool>()))
            .ReturnsAsync(companyUser);

        // Token üretim mock'ları
        _jwtProviderMock.Setup(x => x.CreateTenantTokenAsync(user, companyUser, It.IsAny<CancellationToken>()))
            .ReturnsAsync("access-token-123");
        _jwtProviderMock.Setup(x => x.CreateRefreshToken()).Returns("refresh-token-123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("access-token-123");
        result.Data.RefreshToken.Should().Be("refresh-token-123");
        result.Data.Message.Should().Be("Giriş başarılı.");

        // Db kaydı yapıldı mı?
        _refreshTokenRepositoryMock.Verify(x => x.Add(It.Is<UserRefreshToken>(rt =>
            rt.UserId == user.Id &&
            rt.CompanyUserId == companyUser.Id &&
            rt.Code == "refresh-token-123")), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _userManagerMock.Verify(x => x.ResetAccessFailedCountAsync(user), Times.Once);
    }
}
