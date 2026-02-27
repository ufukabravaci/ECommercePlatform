using ECommercePlatform.Application.Auth.RefreshToken;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using MockQueryable; // Asenkron ToListAsync/FirstOrDefaultAsync mock'u için gerekli
using Moq;

namespace ECommercePlatform.Application.Tests.Auth.RefreshToken;

public class RevokeAllCommandHandlerTests
{
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RevokeAllCommandHandler _handler;

    public RevokeAllCommandHandlerTests()
    {
        _userContextMock = new Mock<IUserContext>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RevokeAllCommandHandler(
            _userContextMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    // Sahte veri üretimi için yardımcı metot
    private User CreateUserWithRefreshTokens(Guid userId)
    {
        var user = new User("Ufuk", "Test", "test@test.com", "test");

        // Private set olan Id'yi reflection ile atıyoruz
        var idProperty = typeof(IdentityUser<Guid>).GetProperty("Id");
        idProperty?.SetValue(user, userId);

        // Kullanıcıya 2 tane aktif refresh token ekliyoruz
        var token1 = new UserRefreshToken
        {
            Code = "token-1",
            Expiration = DateTimeOffset.Now.AddDays(1) // Süresi geçmemiş
        };

        var token2 = new UserRefreshToken
        {
            Code = "token-2",
            Expiration = DateTimeOffset.Now.AddDays(2) // Süresi geçmemiş
        };

        user.AddRefreshToken(token1);
        user.AddRefreshToken(token2);

        return user;
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserIdIsEmpty()
    {
        // Arrange
        // IUserContext boş Guid dönüyor (Token okunmamış veya geçersiz)
        _userContextMock.Setup(x => x.GetUserId()).Returns(Guid.Empty);

        var command = new RevokeAllCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Kullanıcı bulunamadı.");

        // Veritabanına kesinlikle gidilmemiş olmalı
        _userRepositoryMock.Verify(x => x.GetAll(), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserIsNotFoundInDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        // Veritabanında (Mock) böyle bir kullanıcı yok
        var emptyUserList = new List<User>();
        _userRepositoryMock.Setup(x => x.GetAll()).Returns(emptyUserList.BuildMock());

        var command = new RevokeAllCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Kullanıcı bulunamadı.");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_And_RevokeTokens_When_UserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        var user = CreateUserWithRefreshTokens(userId);

        // Veritabanından bu kullanıcı dönüyor
        var userList = new List<User> { user };
        _userRepositoryMock.Setup(x => x.GetAll()).Returns(userList.BuildMock());

        var command = new RevokeAllCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Tüm cihazlardan çıkış yapıldı.");

        // Kullanıcının içindeki domain metodu çalışıp tokenları iptal etmiş mi?
        user.RefreshTokens.Should().HaveCount(2);
        user.RefreshTokens.All(t => t.RevokedAt != null).Should().BeTrue();

        // Veritabanı kaydedilmiş mi?
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
