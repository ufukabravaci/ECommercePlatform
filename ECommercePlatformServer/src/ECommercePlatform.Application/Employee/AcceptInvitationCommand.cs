using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Users;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Employee;

// Login olmuş kullanıcılar içindir, Permission gerekmez.
public sealed record AcceptInvitationCommand(string Token) : IRequest<Result<string>>;

public sealed class AcceptInvitationCommandHandler(
    ICompanyInvitationRepository invitationRepository,
    ICompanyUserRepository companyUserRepository,
    IUserContext userContext, // Login olmuş kullanıcının ID'si
    IUnitOfWork unitOfWork
) : IRequestHandler<AcceptInvitationCommand, Result<string>>
{
    public async Task<Result<string>> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        Guid currentUserId = userContext.GetUserId();
        if (currentUserId == Guid.Empty)
            return Result<string>.Failure("Lütfen önce giriş yapınız.");

        // 1. Davet Kontrol
        var invitation = await invitationRepository.FirstOrDefaultAsync(
            x => x.Token == request.Token, cancellationToken);

        if (invitation is null) return Result<string>.Failure("Geçersiz davet.");
        if (invitation.Status != InvitationStatus.Pending) return Result<string>.Failure("Davet artık aktif değil.");
        if (DateTimeOffset.Now > invitation.ExpirationDate) return Result<string>.Failure("Davet süresi dolmuş.");

        // 2. Zaten üye mi?
        bool isAlreadyMember = await companyUserRepository.AnyAsync(
            x => x.UserId == currentUserId && x.CompanyId == invitation.CompanyId,
            cancellationToken);

        if (isAlreadyMember)
            return Result<string>.Failure("Zaten bu şirketin çalışanısınız.");

        // 3. Şirkete Ekle (Davetiyedeki Rol ile)
        var companyUser = new CompanyUser(currentUserId, invitation.CompanyId);

        // ÖNEMLİ: Hardcode yok, davetteki rolü veriyoruz.
        companyUser.AddRole(invitation.Role);

        companyUserRepository.Add(companyUser);

        // 4. Daveti Kapat
        invitation.MarkAsAccepted();
        invitationRepository.Update(invitation);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Daveti kabul ettiniz, şirkete katılım sağlandı.");
    }
}