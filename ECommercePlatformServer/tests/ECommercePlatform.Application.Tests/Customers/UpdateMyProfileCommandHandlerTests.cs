using ECommercePlatform.Application.Profile;
using ECommercePlatform.Application.Tests.TestHelpers;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;

namespace ECommercePlatform.Application.Tests.Profile;

public class UpdateMyProfileCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly UpdateMyProfileCommandHandler _handler;

    public UpdateMyProfileCommandHandlerTests()
    {
        _userManagerMock = IdentityMocks.CreateMockUserManager<User>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _handler = new UpdateMyProfileCommandHandler(_userManagerMock.Object, _httpContextAccessorMock.Object);
    }

    private void SetupHttpContextWithUserId(string userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // DefaultHttpContext YERİNE Mock<HttpContext> kullanıyoruz
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.User).Returns(claimsPrincipal);

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_HttpContextUserIsInvalid()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);

        var command = new UpdateMyProfileCommand("Test", "User", null, null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Kullanıcı oturumu geçersiz.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserAndAddress_When_Valid()
    {
        var userId = Guid.NewGuid().ToString();
        SetupHttpContextWithUserId(userId);

        var user = new User("Eski", "Ad", "test@test.com", "eski_kullanici");
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // HATA BURADAYDI: AddressDto'nun projende DTOs altında tanımlandığını veya senin gönderdiğin parametre sırasının yanlış olduğunu varsayıyorum.
        // Senin kodlarında AddressDto record yapısı şu şekilde varsayılıyor: AddressDto(City, District, Street, ZipCode, FullAddress)
        var newAddress = new AddressDto("Ankara", "Kadıköy", "Sokak", "06000", "Test Adres");

        var command = new UpdateMyProfileCommand("Yeni", "Soyad", "555123", newAddress);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Profil başarıyla güncellendi.");

        user.FirstName.Should().Be("Yeni");
        user.LastName.Should().Be("Soyad");
        user.PhoneNumber.Should().Be("555123");
        user.Address.Should().NotBeNull();
        user.Address!.City.Should().Be("Ankara");

        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}
