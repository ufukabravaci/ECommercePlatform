using ECommercePlatform.Application.Auth;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Application.Tests.TestHelpers;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ECommercePlatform.Application.Tests.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IEmailService> _mailServiceMock;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _userManagerMock = IdentityMocks.CreateMockUserManager<User>();
        _mailServiceMock = new Mock<IEmailService>();

        _handler = new ForgotPasswordCommandHandler(
            _userManagerMock.Object,
            _mailServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnGenericMessage_And_NotSendEmail_When_UserNotFound()
    {
        // Arrange
        var command = new ForgotPasswordCommand("notfound@test.com");

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((User?)null); // Kullanıcı yok

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue(); // Dikkat: Güvenlik gereği true dönüyor!
        result.Data.Should().Be("Eğer sistemde kayıtlıysa, şifre sıfırlama linki gönderildi.");

        // Önemli: Gerçekte mail atılmamış olmalı (Boşuna SMTP masrafı veya hata olmasın)
        _mailServiceMock.Verify(
            x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_And_SendEmail_When_UserExists()
    {
        // Arrange
        var command = new ForgotPasswordCommand("ufuk@test.com");
        var user = new User("Ufuk", "Test", command.Email, command.Email);
        var expectedToken = "reset-token-123";

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Şifre sıfırlama maili gönderildi.");

        // Mail servisi doğru e-posta adresine ve içeriğinde token olacak şekilde çağrıldı mı?
        _mailServiceMock.Verify(
            x => x.SendAsync(
                command.Email,
                "Şifre Sıfırlama Kodu",
                It.Is<string>(body => body.Contains(expectedToken)), // İçerikte token var mı?
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
