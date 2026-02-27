using ECommercePlatform.Application.Auth.RefreshToken;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using MockQueryable; // BuildMock() için gerekli
using Moq;

namespace ECommercePlatform.Application.Tests.Auth.RefreshToken;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _jwtProviderMock = new Mock<IJwtProvider>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _handler = new RefreshTokenCommandHandler(
            _jwtProviderMock.Object,
            _unitOfWorkMock.Object,
            _userRepositoryMock.Object
        );
    }

    // Gerekli ilişkisel sahte veriyi hazırlayan yardımcı metot
    private User CreateUserWithRefreshToken(string tokenCode, bool isTokenActive, bool includeCompanyUser)
    {
        var userId = Guid.NewGuid();
        var user = new User("Ufuk", "Test", "test@test.com", "test");
        var idProperty = typeof(IdentityUser<Guid>).GetProperty("Id");
        idProperty?.SetValue(user, userId);

        var companyUser = new CompanyUser(userId, Guid.NewGuid());
        var entityIdProperty = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        entityIdProperty?.SetValue(companyUser, Guid.NewGuid());

        var token = new UserRefreshToken
        {
            Code = tokenCode,
            Expiration = isTokenActive ? DateTimeOffset.Now.AddDays(1) : DateTimeOffset.Now.AddDays(-1),
            UserId = userId,
            CompanyUserId = companyUser.Id,
            CompanyUser = includeCompanyUser ? companyUser : null! // Navigation property
        };



        // Reflection ile Entity.IsActive değerini değiştirelim (Base Entity'den geliyor)
        var isActiveProp = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("IsActive");
        isActiveProp?.SetValue(token, isTokenActive);

        user.AddRefreshToken(token);
        return user;
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserNotFound()
    {
        // Arrange
        var command = new RefreshTokenCommand("non-existent-token");

        // Veritabanında (Mock) hiçbir şey yok
        var emptyUserList = new List<User>();
        _userRepositoryMock.Setup(x => x.GetAll()).Returns(emptyUserList.BuildMock());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Token geçersiz.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_TokenIsNotActive()
    {
        // Arrange
        var command = new RefreshTokenCommand("expired-token");
        var user = CreateUserWithRefreshToken(command.RefreshToken, isTokenActive: false, includeCompanyUser: true);

        var userList = new List<User> { user };
        _userRepositoryMock.Setup(x => x.GetAll()).Returns(userList.BuildMock());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Token geçersiz veya süresi dolmuş.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CompanyUserIsNull()
    {
        // Arrange
        var command = new RefreshTokenCommand("valid-token-no-tenant");
        var user = CreateUserWithRefreshToken(command.RefreshToken, isTokenActive: true, includeCompanyUser: false);

        var userList = new List<User> { user };
        _userRepositoryMock.Setup(x => x.GetAll()).Returns(userList.BuildMock());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Oturum bilgisi (Tenant) bulunamadı. Lütfen tekrar giriş yapın.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_When_TokenIsValid()
    {
        // Arrange
        var command = new RefreshTokenCommand("valid-token-123");
        var user = CreateUserWithRefreshToken(command.RefreshToken, isTokenActive: true, includeCompanyUser: true);

        var userList = new List<User> { user };
        _userRepositoryMock.Setup(x => x.GetAll()).Returns(userList.BuildMock());

        _jwtProviderMock
            .Setup(x => x.CreateTenantTokenAsync(It.IsAny<User>(), It.IsAny<CompanyUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("new-access-token");

        _jwtProviderMock
            .Setup(x => x.CreateRefreshToken())
            .Returns("new-refresh-token-456");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("new-access-token");
        result.Data.RefreshToken.Should().Be("new-refresh-token-456");

        // Token döndürülmüş mü?
        var existingToken = user.RefreshTokens.Single(t => t.Code == command.RefreshToken);
        existingToken.RevokedAt.Should().NotBeNull();
        existingToken.ReplacedByToken.Should().Be("new-refresh-token-456");

        // Yeni token user'a eklenmiş mi?
        user.RefreshTokens.Any(t => t.Code == "new-refresh-token-456").Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
