using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Options;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using GenericRepository;
using Microsoft.Extensions.Options;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Employee;

[Permission(PermissionConsts.InviteEmployee)]
public sealed record SendInvitationCommand(
    string Email,
    string Role // Hangi yetkiyle gelecek?
) : IRequest<Result<string>>;

public sealed class SendInvitationCommandHandler(
    ITenantContext tenantContext,
    ICompanyInvitationRepository invitationRepository,
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    IOptions<ClientSettings> clientSettings // MVC Url'ini buradan alıyoruz
) : IRequestHandler<SendInvitationCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SendInvitationCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.CompanyId is null)
            return Result<string>.Failure("Şirket bilgisi bulunamadı.");

        // 1. Aynı mail için bekleyen davet var mı?
        var existingInvite = await invitationRepository.FirstOrDefaultAsync(
            x => x.Email == request.Email &&
                 x.CompanyId == tenantContext.CompanyId &&
                 x.Status == InvitationStatus.Pending,
            cancellationToken);

        if (existingInvite != null)
            return Result<string>.Failure("Bu kişiye zaten gönderilmiş ve bekleyen bir davet var.");

        // 2. Davet Oluştur
        var invitation = new CompanyInvitation(
            tenantContext.CompanyId.Value,
            request.Email,
            request.Role // Seçilen rolü kaydediyoruz
        );

        invitationRepository.Add(invitation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Link Oluştur (MVC Adresi)
        // Link: https://localhost:7277/Invite/Accept?token=xyz...
        string baseUrl = clientSettings.Value.Url;
        string inviteLink = $"{baseUrl}/Invite/Accept?token={invitation.Token}";

        // 4. Mail Gönder
        await emailService.SendAsync(
            request.Email,
            "Ekip Daveti",
            $@"<p>Sizi ekibimize <strong>{request.Role}</strong> olarak davet ediyoruz.</p>
               <p>Kabul etmek için tıklayın: <a href='{inviteLink}'>Daveti Kabul Et</a></p>",
            cancellationToken
        );

        return Result<string>.Succeed("Davet başarıyla gönderildi.");
    }
}