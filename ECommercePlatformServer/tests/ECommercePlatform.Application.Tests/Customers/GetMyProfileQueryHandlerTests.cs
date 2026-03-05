using ECommercePlatform.Application.Profile;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Application.Tests.TestHelpers;
using ECommercePlatform.Domain.Users;
using ECommercePlatform.Domain.Users.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ECommercePlatform.Application.Tests.Profile;

public class GetMyProfileQueryHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly GetMyProfileQueryHandler _handler;

    public GetMyProfileQueryHandlerTests()
    {
        _userManagerMock = IdentityMocks.CreateMockUserManager<User>();
        _userContextMock = new Mock<IUserContext>();

        _handler = new GetMyProfileQueryHandler(_userManagerMock.Object, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserIdIsEmpty()
    {
        _userContextMock.Setup(x => x.GetUserId()).Returns(Guid.Empty);

        var result = await _handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        // Not: Handler "Guid.Empty.ToString()" yapınca Empty Guid string döner. 
        // string.IsNullOrEmpty(userIdString) kontrolü Guid.Empty için ("0000...") false döner!
        // Bu yüzden veritabanında bulamayacaktır.

        // Eğer sistemde Guid.Empty'yi bulamazsa
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Kullanıcı veritabanında bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnProfile_When_UserExists()
    {
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        var user = new User("Ufuk", "Abravacı", "test@test.com", "ufuk123");
        user.PhoneNumber = "1234567890";
        user.SetAddress(new Address("İstanbul", "Kadıköy", "Moda", "34000", "Tam adres"));

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        var result = await _handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FirstName.Should().Be("Ufuk");
        result.Data.Email.Should().Be("test@test.com");
        result.Data.Address.Should().NotBeNull();
        result.Data.Address!.City.Should().Be("İstanbul");
    }
}
