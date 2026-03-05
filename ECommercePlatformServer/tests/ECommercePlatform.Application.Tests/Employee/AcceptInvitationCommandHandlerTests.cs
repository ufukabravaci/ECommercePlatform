using ECommercePlatform.Application.Employee;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Employee;

public class AcceptInvitationCommandHandlerTests
{
    private readonly Mock<ICompanyInvitationRepository> _invitationRepoMock;
    private readonly Mock<ICompanyUserRepository> _companyUserRepoMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AcceptInvitationCommandHandler _handler;

    public AcceptInvitationCommandHandlerTests()
    {
        _invitationRepoMock = new Mock<ICompanyInvitationRepository>();
        _companyUserRepoMock = new Mock<ICompanyUserRepository>();
        _userContextMock = new Mock<IUserContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new AcceptInvitationCommandHandler(
            _invitationRepoMock.Object,
            _companyUserRepoMock.Object,
            _userContextMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_UserIsNotLoggedIn()
    {
        _userContextMock.Setup(x => x.GetUserId()).Returns(Guid.Empty);

        var command = new AcceptInvitationCommand("test-token");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Lütfen önce giriş yapınız.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_InvitationNotFound()
    {
        _userContextMock.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        _invitationRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyInvitation, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                           .ReturnsAsync((CompanyInvitation)null!);

        var command = new AcceptInvitationCommand("test-token");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Geçersiz davet.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_AlreadyMember()
    {
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        var invitation = new CompanyInvitation(companyId, "test@test.com", "Employee");

        _invitationRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyInvitation, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                           .ReturnsAsync(invitation);

        // Kullanıcı zaten o şirketin bir üyesi!
        _companyUserRepoMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyUser, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);

        var command = new AcceptInvitationCommand("test-token");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Zaten bu şirketin çalışanısınız.");
    }

    [Fact]
    public async Task Handle_ShouldAcceptInvitationAndCreateCompanyUser_When_Valid()
    {
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserId()).Returns(userId);

        var invitation = new CompanyInvitation(companyId, "test@test.com", "Employee");

        _invitationRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyInvitation, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                           .ReturnsAsync(invitation);

        // Üye değil
        _companyUserRepoMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyUser, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);

        var command = new AcceptInvitationCommand("test-token");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Daveti kabul ettiniz, şirkete katılım sağlandı.");

        // Davet "Accepted" durumuna çekilmiş mi?
        invitation.Status.Should().Be(InvitationStatus.Accepted);

        // Doğru metodlar çağrılmış mı?
        _companyUserRepoMock.Verify(x => x.Add(It.Is<CompanyUser>(c => c.UserId == userId && c.CompanyId == companyId)), Times.Once);
        _invitationRepoMock.Verify(x => x.Update(invitation), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
