using ECommercePlatform.Application.Auth;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Application.Tests.TestHelpers;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ECommercePlatform.Application.Tests.Auth;

public class ToggleTwoFactorCommandHandlerTests
{
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly ToggleTwoFactorCommandHandler _handler;

    public ToggleTwoFactorCommandHandlerTests()
    {
        _userContextMock = new Mock<IUserContext>();
        _userManagerMock = IdentityMocks.CreateMockUserManager<User>();

        _handler = new ToggleTwoFactorCommandHandler(
            _userContextMock.Object,
            _userManagerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        // Kullanıcı veritabanında yok
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((User?)null);

        var command = new ToggleTwoFactorCommand(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Kullanıcı bulunamadı.");

        // Identity metoduna gidilmemiş olmalı
        _userManagerMock.Verify(x => x.SetTwoFactorEnabledAsync(It.IsAny<User>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_IdentityOperationFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        var user = new User("Ufuk", "Test", "test@test.com", "test");
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        // Identity hata döndürüyor
        var identityError = new IdentityError { Description = "2FA ayarı değiştirilemedi." };
        _userManagerMock
            .Setup(x => x.SetTwoFactorEnabledAsync(user, true))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var command = new ToggleTwoFactorCommand(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("2FA ayarı değiştirilemedi.");
    }

    [Theory]
    [InlineData(true, "aktif edildi")]
    [InlineData(false, "pasif edildi")]
    public async Task Handle_ShouldReturnSuccess_With_CorrectMessage_BasedOnEnableFlag(bool enableFlag, string expectedStatus)
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        var user = new User("Ufuk", "Test", "test@test.com", "test");
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        // Identity işlemi başarılı
        _userManagerMock
            .Setup(x => x.SetTwoFactorEnabledAsync(user, enableFlag))
            .ReturnsAsync(IdentityResult.Success);

        var command = new ToggleTwoFactorCommand(enableFlag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be($"İki aşamalı doğrulama başarıyla {expectedStatus}.");

        _userManagerMock.Verify(x => x.SetTwoFactorEnabledAsync(user, enableFlag), Times.Once);
    }
}
