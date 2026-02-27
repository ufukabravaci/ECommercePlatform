using ECommercePlatform.Application.Auth;
using ECommercePlatform.Application.Tests.TestHelpers;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using MockQueryable; // BuildMock() için gerekli
using Moq;

namespace ECommercePlatform.Application.Tests.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IUserRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _userManagerMock = IdentityMocks.CreateMockUserManager<User>();
        _refreshTokenRepositoryMock = new Mock<IUserRefreshTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new ResetPasswordCommandHandler(
            _userManagerMock.Object,
            _refreshTokenRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserNotFound()
    {
        // Arrange
        var command = new ResetPasswordCommand("notfound@test.com", "token", "NewPass123", "NewPass123");

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Kullanıcı bulunamadı.");

        // Identity.ResetPasswordAsync veya veritabanı kaydı çağrılmamalı
        _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_ResetPasswordFails()
    {
        // Arrange
        var command = new ResetPasswordCommand("test@test.com", "invalid-token", "NewPass123", "NewPass123");
        var user = new User("Ufuk", "Test", command.Email, command.Email);

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(user);

        // Identity Result başarısız dönüyor
        var identityError = new IdentityError { Description = "Invalid Token." };
        _userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Invalid Token.");

        // Veritabanı token temizleme adımına geçilmemiş olmalı
        _refreshTokenRepositoryMock.Verify(x => x.WhereWithTracking(It.IsAny<System.Linq.Expressions.Expression<Func<UserRefreshToken, bool>>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_And_RevokeActiveTokens_When_Successful()
    {
        // Arrange
        var command = new ResetPasswordCommand("test@test.com", "valid-token", "NewPass123", "NewPass123");
        var userId = Guid.NewGuid();
        var user = new User("Ufuk", "Test", command.Email, command.Email);

        // Id setleme
        var idProperty = typeof(IdentityUser<Guid>).GetProperty("Id");
        idProperty?.SetValue(user, userId);

        // 2 adet aktif token oluştur
        var activeToken1 = new UserRefreshToken { UserId = userId, Code = "token-1", Expiration = DateTimeOffset.Now.AddDays(1) };
        var activeToken2 = new UserRefreshToken { UserId = userId, Code = "token-2", Expiration = DateTimeOffset.Now.AddDays(2) };
        var activeTokensList = new List<UserRefreshToken> { activeToken1, activeToken2 };

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Veritabanı sorgusunu (WhereWithTracking) mockla
        _refreshTokenRepositoryMock
            .Setup(x => x.WhereWithTracking(It.IsAny<System.Linq.Expressions.Expression<Func<UserRefreshToken, bool>>>()))
            .Returns(activeTokensList.BuildMock()); // MockQueryable IQueryable'a çevirir

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Şifreniz başarıyla güncellendi. Yeni şifrenizle giriş yapabilirsiniz.");

        // Aktif tokenların hepsi revoke edilmiş mi? (Foreach kontrolü)
        activeToken1.RevokedAt.Should().NotBeNull();
        activeToken1.ReplacedByToken.Should().BeNull();

        activeToken2.RevokedAt.Should().NotBeNull();
        activeToken2.ReplacedByToken.Should().BeNull();

        // Veritabanına kaydedildi mi?
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
