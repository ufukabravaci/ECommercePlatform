using ECommercePlatform.Application.Employee;
using ECommercePlatform.Application.Options; // ClientSettings için
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Employee;

public class SendInvitationCommandHandlerTests
{
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ICompanyInvitationRepository> _invitationRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SendInvitationCommandHandler _handler;

    public SendInvitationCommandHandlerTests()
    {
        _tenantContextMock = new Mock<ITenantContext>();
        _invitationRepoMock = new Mock<ICompanyInvitationRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var options = Microsoft.Extensions.Options.Options.Create(new ClientSettings { Url = "https://localhost:7277" });

        _handler = new SendInvitationCommandHandler(
            _tenantContextMock.Object,
            _invitationRepoMock.Object,
            _emailServiceMock.Object,
            _unitOfWorkMock.Object,
            options
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CompanyIdIsNull()
    {
        _tenantContextMock.Setup(x => x.CompanyId).Returns((Guid?)null);

        var command = new SendInvitationCommand("test@test.com", "Employee");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Şirket bilgisi bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_PendingInvitationAlreadyExists()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var existingInvite = new CompanyInvitation(companyId, "test@test.com", "Employee");

        _invitationRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyInvitation, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                           .ReturnsAsync(existingInvite);

        var command = new SendInvitationCommand("test@test.com", "Employee");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu kişiye zaten gönderilmiş ve bekleyen bir davet var.");
    }

    [Fact]
    public async Task Handle_ShouldCreateInvitationAndSendEmail_When_Valid()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        // Bekleyen davet yok
        _invitationRepoMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CompanyInvitation, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                           .ReturnsAsync((CompanyInvitation)null!);

        var command = new SendInvitationCommand("test@test.com", "Employee");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Davet başarıyla gönderildi.");

        _invitationRepoMock.Verify(x => x.Add(It.Is<CompanyInvitation>(c => c.Email == "test@test.com")), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Email gönderilmiş mi?
        _emailServiceMock.Verify(x => x.SendAsync("test@test.com", "Ekip Daveti", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
