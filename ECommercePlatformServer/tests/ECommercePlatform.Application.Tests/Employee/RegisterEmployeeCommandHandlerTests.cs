using ECommercePlatform.Application.Employee;
using ECommercePlatform.Application.Tests.TestHelpers;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using GenericRepository;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ECommercePlatform.Application.Tests.Employee;

public class RegisterEmployeeCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<ICompanyInvitationRepository> _invitationRepoMock;
    private readonly Mock<ICompanyUserRepository> _companyUserRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterEmployeeCommandHandler _handler;

    public RegisterEmployeeCommandHandlerTests()
    {
        _userManagerMock = IdentityMocks.CreateMockUserManager<User>();
        _invitationRepoMock = new Mock<ICompanyInvitationRepository>();
        _companyUserRepoMock = new Mock<ICompanyUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RegisterEmployeeCommandHandler(
            _userManagerMock.Object,
            _invitationRepoMock.Object,
            _companyUserRepoMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_InvitationNotFound()
    {
        var command = new RegisterEmployeeCommand("token", "Ad", "Soyad", "123456", "123456");

        _invitationRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyInvitation, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                           .ReturnsAsync((CompanyInvitation)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Geçersiz veya hatalı davet kodu.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserAlreadyExists()
    {
        var command = new RegisterEmployeeCommand("token", "Ad", "Soyad", "123456", "123456");
        var invitation = new CompanyInvitation(Guid.NewGuid(), "test@test.com", "Employee");

        _invitationRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyInvitation, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                           .ReturnsAsync(invitation);

        // Zaten aynı e-posta ile kayıtlı biri var!
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@test.com")).ReturnsAsync(new User("Test", "Test", "test@test.com", "test"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu e-posta adresiyle zaten bir üyelik mevcut. Lütfen giriş yaparak daveti kabul ediniz.");
    }

    [Fact]
    public async Task Handle_ShouldCreateUserAndAcceptInvitation_When_Valid()
    {
        var command = new RegisterEmployeeCommand("token", "Ad", "Soyad", "123456", "123456");
        var companyId = Guid.NewGuid();
        var invitation = new CompanyInvitation(companyId, "test@test.com", "Employee");

        _invitationRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyInvitation, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                           .ReturnsAsync(invitation);

        _userManagerMock.Setup(x => x.FindByEmailAsync("test@test.com")).ReturnsAsync((User)null!);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), "123456")).ReturnsAsync(IdentityResult.Success);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Contain("Giriş yapabilirsiniz.");

        invitation.Status.Should().Be(InvitationStatus.Accepted);

        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), "123456"), Times.Once);
        _companyUserRepoMock.Verify(x => x.Add(It.IsAny<CompanyUser>()), Times.Once);
        _invitationRepoMock.Verify(x => x.Update(invitation), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
