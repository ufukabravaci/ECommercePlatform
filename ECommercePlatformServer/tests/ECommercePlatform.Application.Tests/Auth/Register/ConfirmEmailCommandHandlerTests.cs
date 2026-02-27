using ECommercePlatform.Application.Auth.Register;
using ECommercePlatform.Application.Tests.TestHelpers;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ECommercePlatform.Application.Tests.Auth.Register;

public class ConfirmEmailCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly ConfirmEmailCommandHandler _handler;

    public ConfirmEmailCommandHandlerTests()
    {
        _userManagerMock = IdentityMocks.CreateMockUserManager<User>();
        _handler = new ConfirmEmailCommandHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserNotFound()
    {
        var command = new ConfirmEmailCommand("notfound@test.com", "token-123");
        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Kullanıcı bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_TokenIsInvalid()
    {
        var command = new ConfirmEmailCommand("test@test.com", "invalid-token");
        var user = new User("Test", "User", command.Email, command.Email);

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, command.Token))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid Token" }));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Email doğrulanamadı. Token geçersiz veya süresi dolmuş.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_When_TokenIsValid()
    {
        var command = new ConfirmEmailCommand("test@test.com", "valid-token");
        var user = new User("Test", "User", command.Email, command.Email);

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, command.Token)).ReturnsAsync(IdentityResult.Success);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Email başarıyla doğrulandı. Giriş yapabilirsiniz.");
    }
}
